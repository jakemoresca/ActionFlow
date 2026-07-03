using ActionFlow.DB.Documents;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine.Providers;
using Marten;

namespace ActionFlow.DB.Providers;

/// <summary>
/// Marten-backed <see cref="IWorkflowProvider"/> — the default provider for the ActionFlow.Runner.
/// Resolves workflows by name (latest version) or by an explicit version, without materializing the
/// whole store on the hot path.
/// </summary>
public class DocumentStoreWorkflowProvider(IDocumentStore store) : IWorkflowProvider
{
    /// <summary>
    /// Returns the latest version of every workflow. Backward-compatibility path; prefer
    /// <see cref="GetWorkflowAsync(string, CancellationToken)"/> for single lookups.
    /// </summary>
    public List<Workflow> GetAllWorkflows()
    {
        using var session = store.QuerySession();
        return session.Query<WorkflowDocument>()
            .Where(x => x.IsLatest)
            .ToList()
            .Select(x => x.ToWorkflow())
            .ToList();
    }

    public async Task<Workflow?> GetWorkflowAsync(string name, CancellationToken cancellationToken = default)
    {
        await using var session = store.QuerySession();
        var document = await session.Query<WorkflowDocument>()
            .Where(x => x.Name == name && x.IsLatest)
            .FirstOrDefaultAsync(cancellationToken);

        return document?.ToWorkflow();
    }

    /// <summary>Resolves a specific pinned version of a workflow.</summary>
    public async Task<Workflow?> GetWorkflowAsync(string name, int version, CancellationToken cancellationToken = default)
    {
        await using var session = store.QuerySession();
        var document = await session.LoadAsync<WorkflowDocument>(WorkflowDocument.MakeId(name, version), cancellationToken);

        return document?.ToWorkflow();
    }
}
