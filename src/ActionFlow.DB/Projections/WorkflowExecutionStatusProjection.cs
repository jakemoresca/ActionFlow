using ActionFlow.Contracts;
using ActionFlow.DB.ReadModels;
using Marten.Events.Aggregation;

namespace ActionFlow.DB.Projections;

/// <summary>
/// Single-stream projection folding an execution's event stream into a <see cref="WorkflowExecutionStatus"/>.
/// The stream is keyed by <c>ExecutionId</c>. Registered async so it can be rebuilt from history.
/// </summary>
public partial class WorkflowExecutionStatusProjection : SingleStreamProjection<WorkflowExecutionStatus, Guid>
{
    public static WorkflowExecutionStatus Create(WorkflowExecutionRequested @event) => new()
    {
        Id = @event.ExecutionId,
        WorkflowName = @event.WorkflowName,
        Version = @event.Version,
        Status = "Requested",
        RequestedAt = @event.RequestedAt
    };

    public void Apply(WorkflowExecutionStarted @event, WorkflowExecutionStatus status)
    {
        status.Status = "Running";
        status.StartedAt = @event.StartedAt;
    }

    public void Apply(StepCompleted @event, WorkflowExecutionStatus status)
    {
        status.CurrentStepIndex = @event.Index;
        status.CompletedSteps.Add(@event.StepName);
    }

    public void Apply(StepFailed @event, WorkflowExecutionStatus status)
    {
        status.CurrentStepIndex = @event.Index;
        status.Error = @event.Error;
    }

    public void Apply(WorkflowExecutionCompleted @event, WorkflowExecutionStatus status)
    {
        status.Status = "Completed";
        status.OutputParameters = @event.OutputParameters;
        status.CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Apply(WorkflowExecutionFailed @event, WorkflowExecutionStatus status)
    {
        status.Status = "Failed";
        status.Error = @event.Error;
        status.CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Apply(WorkflowExecutionCompensated @event, WorkflowExecutionStatus status)
    {
        status.Status = "Compensated";
        status.CompletedAt = DateTimeOffset.UtcNow;
    }
}
