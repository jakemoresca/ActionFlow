using ActionFlow.Contracts;
using Marten;

namespace ActionFlow.Api.Handlers;

/// <summary>
/// Consumes the execution events the Runner emits and appends them to the execution's Marten event
/// stream (keyed by ExecutionId). The inline <c>WorkflowExecutionStatus</c> projection folds them into
/// the read model, and the raw stream backs the debugging timeline. Wolverine's Marten integration
/// commits the session after each handler, so appends are transactional.
/// The stream is created by the execute endpoint (WorkflowExecutionRequested), which always precedes
/// these events causally.
/// </summary>
public class ExecutionEventStreamHandler
{
    public void Handle(WorkflowExecutionStarted @event, IDocumentSession session)
        => session.Events.Append(@event.ExecutionId, @event);

    public void Handle(StepCompleted @event, IDocumentSession session)
        => session.Events.Append(@event.ExecutionId, @event);

    public void Handle(StepFailed @event, IDocumentSession session)
        => session.Events.Append(@event.ExecutionId, @event);

    public void Handle(WorkflowExecutionCompleted @event, IDocumentSession session)
        => session.Events.Append(@event.ExecutionId, @event);

    public void Handle(WorkflowExecutionFailed @event, IDocumentSession session)
        => session.Events.Append(@event.ExecutionId, @event);

    public void Handle(WorkflowExecutionCompensated @event, IDocumentSession session)
        => session.Events.Append(@event.ExecutionId, @event);
}
