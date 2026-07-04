using ActionFlow.Contracts;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using Microsoft.Extensions.Logging;
using Wolverine;
using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Runner.Handlers;

/// <summary>
/// Consumes <see cref="ExecuteWorkflow"/>: runs the workflow on the hosted engine and emits execution
/// events. Step-level events are emitted by <see cref="Observers.MessagingStepObserver"/>; this handler
/// emits the started/terminal events. (No saga yet — that arrives in Phase 3.)
/// </summary>
public class ExecuteWorkflowHandler
{
    public async Task Handle(
        ExecuteWorkflow command,
        IActionFlowEngine engine,
        ExecutionEventBuffer stepEvents,
        IMessageBus bus,
        ILogger<ExecuteWorkflowHandler> logger)
    {
        var delivery = new DeliveryOptions { PartitionKey = command.ExecutionId.ToString() };

        await bus.PublishAsync(
            new WorkflowExecutionStarted { ExecutionId = command.ExecutionId, StartedAt = DateTimeOffset.UtcNow },
            delivery);

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
            // context so they share one outbox and stay ordered ahead of the completion event.
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

            // Deliberately not rethrown: coarse retry/DLQ and saga-driven compensation land in Phase 3.
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
