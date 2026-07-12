using ActionFlow.Contracts;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ActionFlow.Runner.Sagas;
using Microsoft.Extensions.Logging;
using Wolverine;
using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Runner.Handlers;

/// <summary>
/// Runs a workflow on the hosted engine in response to the saga's <see cref="RunWorkflowExecution"/>
/// command, emitting the per-step <see cref="StepCompleted"/> events (buffered by
/// <see cref="Observers.BufferingStepObserver"/>) and the terminal
/// <see cref="WorkflowExecutionCompleted"/>/<see cref="WorkflowExecutionFailed"/> event. Those events
/// carry the saga id (stamped by Wolverine on this handler's context) so they route back to the saga.
/// The <see cref="WorkflowExecutionStarted"/> event is owned by the saga's start.
/// </summary>
public class RunWorkflowExecutionHandler
{
    public async Task Handle(
        RunWorkflowExecution command,
        IActionFlowEngine engine,
        ExecutionEventBuffer stepEvents,
        IMessageBus bus,
        ILogger<RunWorkflowExecutionHandler> logger)
    {
        var delivery = new DeliveryOptions { PartitionKey = command.ExecutionId.ToString() };

        // Build the context ourselves (rather than the params overload) so we can attach the
        // ExecutionId that the step observer tags its events with.
        var executionContext = new ExecutionContext(engine) { ExecutionId = command.ExecutionId };
        foreach (var input in command.Inputs)
        {
            executionContext.AddOrUpdateParameter(new Parameter { Name = input.Key, Expression = input.Value });
        }

        try
        {
            var result = await engine.ExecuteWorkflowAsync(command.WorkflowName, executionContext);

            // Publish the buffered per-step events, then the terminal event, all via this handler's
            // context so they share one outbox, keep the ExecutionId partition key, and stay ordered
            // ahead of the completion event.
            await PublishStepEventsAsync(stepEvents, bus, delivery);

            var outputs = result.OutputParameters
                .ToDictionary(entry => entry.Key, entry => entry.Value?.ToString() ?? string.Empty);

            await bus.PublishAsync(
                new WorkflowExecutionCompleted { ExecutionId = command.ExecutionId, OutputParameters = outputs },
                delivery);

            logger.LogInformation("Workflow {Workflow} execution {ExecutionId} completed with {Count} output(s)",
                command.WorkflowName, command.ExecutionId, outputs.Count);
        }
        catch (Exception exception)
        {
            // The buffer holds the StepCompleted events plus the StepFailed for the failing step.
            await PublishStepEventsAsync(stepEvents, bus, delivery);

            await bus.PublishAsync(
                new WorkflowExecutionFailed
                {
                    ExecutionId = command.ExecutionId,
                    Error = exception.Message,
                    LastCompletedStepIndex = executionContext.CurrentStepIndex - 1
                },
                delivery);

            logger.LogError(exception, "Workflow {Workflow} execution {ExecutionId} failed at step index {Index}",
                command.WorkflowName, command.ExecutionId, executionContext.CurrentStepIndex);

            // Not rethrown: the saga observes WorkflowExecutionFailed and drives compensation.
        }
    }

    private static async Task PublishStepEventsAsync(ExecutionEventBuffer stepEvents, IMessageBus bus, DeliveryOptions delivery)
    {
        foreach (var stepEvent in stepEvents.Events)
        {
            await bus.PublishAsync(stepEvent, delivery);
        }
    }
}
