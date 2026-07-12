using ActionFlow.Contracts;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine.Observers;
using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Runner.Observers;

/// <summary>
/// Step observer (§8.2) that records <see cref="StepCompleted"/>/<see cref="StepFailed"/> events into
/// the scoped <see cref="ExecutionEventBuffer"/>. The <c>ExecuteWorkflowHandler</c> drains the buffer
/// and publishes the events through its own message context so they share the handler's outbox and
/// the <c>ExecutionId</c> partition key.
/// </summary>
public class BufferingStepObserver(ExecutionEventBuffer buffer) : IStepExecutionObserver
{
    public ValueTask OnStepStartingAsync(Step step, ExecutionContext context, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public ValueTask OnStepCompletedAsync(Step step, ExecutionContext context, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        if (context.ExecutionId is { } executionId)
        {
            buffer.Add(new StepCompleted
            {
                ExecutionId = executionId,
                StepName = step.Name,
                Index = context.CurrentStepIndex,
                DurationMs = (long)duration.TotalMilliseconds
            });
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask OnStepFailedAsync(Step step, ExecutionContext context, Exception exception, CancellationToken cancellationToken = default)
    {
        if (context.ExecutionId is { } executionId)
        {
            buffer.Add(new StepFailed
            {
                ExecutionId = executionId,
                StepName = step.Name,
                Index = context.CurrentStepIndex,
                Error = exception.Message
            });
        }

        return ValueTask.CompletedTask;
    }
}
