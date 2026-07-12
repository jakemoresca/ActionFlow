namespace ActionFlow.Runner;

/// <summary>
/// Per-execution (scoped) collector for step events raised during engine execution. The step
/// observer writes here rather than publishing directly, so that the <c>ExecuteWorkflowHandler</c>
/// can publish everything through its own Wolverine message context — keeping all sends inside the
/// same transactional outbox (a separately injected <c>IMessageBus</c> is a detached context and
/// would not be enrolled in the handler's outbox/tracking).
/// </summary>
public sealed class ExecutionEventBuffer
{
    private readonly List<object> _events = new();

    public IReadOnlyList<object> Events => _events;

    public void Add(object @event) => _events.Add(@event);
}
