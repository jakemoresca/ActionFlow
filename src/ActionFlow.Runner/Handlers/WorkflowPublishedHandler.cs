using ActionFlow.Contracts;
using ActionFlow.Engine;
using Microsoft.Extensions.Logging;

namespace ActionFlow.Runner.Handlers;

/// <summary>
/// Consumes <see cref="WorkflowPublished"/> and evicts the workflow from the engine's cache so the
/// next execution reloads the new version from the provider (D4 cache invalidation).
/// </summary>
/// <remarks>
/// The engine is registered scoped, so its in-memory cache is currently per-execution and the
/// Marten-backed provider reads the latest version from the DB each time. This eviction becomes
/// materially useful once a shared/singleton workflow cache is introduced (D4 event-carried cache).
/// </remarks>
public class WorkflowPublishedHandler
{
    public void Handle(WorkflowPublished @event, IActionFlowEngine engine, ILogger<WorkflowPublishedHandler> logger)
    {
        engine.InvalidateWorkflow(@event.Name);
        logger.LogInformation("Evicted workflow {Name} (v{Version}) from the engine cache", @event.Name, @event.Version);
    }
}
