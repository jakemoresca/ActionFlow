using ActionFlow.Contracts;
using Wolverine;

namespace ActionFlow.Runner.Sagas;

/// <summary>
/// Orchestration saga (§7) that owns a single workflow execution's lifecycle. It starts on
/// <see cref="ExecuteWorkflow"/>, delegates the actual run to the local <c>RunWorkflowExecutionHandler</c>
/// via <see cref="RunWorkflowExecution"/>, and reacts to the resulting events — completing, or driving
/// coarse-grained compensation on failure, or failing on timeout.
///
/// Correlation: the saga <see cref="Id"/> is the <c>ExecutionId</c>. Wolverine stamps that id onto all
/// messages cascaded from within the saga's context (and the events those cascades produce), so the
/// execution/step events route back to this saga without any attributes on the contract types.
/// </summary>
public class WorkflowExecutionSaga : Saga
{
    public Guid Id { get; set; }
    public string WorkflowName { get; set; } = default!;

    // Not named "Version": Wolverine's Saga base uses Version for optimistic concurrency.
    public int? WorkflowVersion { get; set; }

    public string Status { get; set; } = SagaStatus.Requested;
    public int CurrentStepIndex { get; set; } = -1;
    public List<string> CompletedSteps { get; set; } = new();
    public DateTimeOffset StartedAt { get; set; }
    public string? Error { get; set; }

    public static (WorkflowExecutionSaga, OutgoingMessages) Start(ExecuteWorkflow command)
    {
        var startedAt = DateTimeOffset.UtcNow;

        var saga = new WorkflowExecutionSaga
        {
            Id = command.ExecutionId,
            WorkflowName = command.WorkflowName,
            WorkflowVersion = command.Version,
            Status = SagaStatus.Running,
            StartedAt = startedAt
        };

        var outgoing = new OutgoingMessages
        {
            new WorkflowExecutionStarted { ExecutionId = command.ExecutionId, StartedAt = startedAt },
            new RunWorkflowExecution(command.ExecutionId, command.WorkflowName, command.Version, command.Inputs),
            new WorkflowExecutionTimeout(command.ExecutionId)
        };

        return (saga, outgoing);
    }

    /// <summary>Track per-step progress for the saga's own view of the execution.</summary>
    public void Handle(StepCompleted @event)
    {
        CurrentStepIndex = @event.Index;
        if (!CompletedSteps.Contains(@event.StepName))
        {
            CompletedSteps.Add(@event.StepName);
        }
    }

    public void Handle(WorkflowExecutionCompleted @event)
    {
        if (SagaStatus.IsTerminal(Status))
        {
            return;
        }

        Status = SagaStatus.Completed;
        MarkCompleted();
    }

    /// <summary>On failure, transition to compensating and issue the compensation command.</summary>
    public object? Handle(WorkflowExecutionFailed @event)
    {
        if (SagaStatus.IsTerminal(Status))
        {
            return null;
        }

        Error = @event.Error;

        // Coarse-grained compensation (§7): compensate the completed steps in reverse. With no
        // compensatable steps recorded there is nothing to undo, so go straight to Failed.
        if (CompletedSteps.Count == 0)
        {
            Status = SagaStatus.Failed;
            MarkCompleted();
            return null;
        }

        Status = SagaStatus.Compensating;
        return new CompensateWorkflow
        {
            ExecutionId = Id,
            ThroughStepIndex = @event.LastCompletedStepIndex
        };
    }

    public void Handle(WorkflowExecutionCompensated @event)
    {
        Status = SagaStatus.Compensated;
        MarkCompleted();
    }

    /// <summary>
    /// Fires only if the deadline elapses before a terminal event (Wolverine discards it once the saga
    /// has completed). Marks the execution failed. It deliberately does not emit
    /// <see cref="WorkflowExecutionFailed"/> — that event is self-handled and would re-enter a saga
    /// that is being completed here; surfacing the timeout to external consumers is a projection concern.
    /// </summary>
    public void Handle(WorkflowExecutionTimeout timeout)
    {
        if (SagaStatus.IsTerminal(Status))
        {
            return;
        }

        Status = SagaStatus.Failed;
        Error = "Execution timed out before completion";
        MarkCompleted();
    }
}
