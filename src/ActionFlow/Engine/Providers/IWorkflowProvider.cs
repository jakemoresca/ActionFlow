using ActionFlow.Domain.Engine;

namespace ActionFlow.Engine.Providers
{
	public interface IWorkflowProvider
	{
		/// <summary>
		/// Loads every known workflow. Kept for backward compatibility and for
		/// in-memory providers; DB-backed providers should prefer <see cref="GetWorkflowAsync"/>
		/// so they don't have to materialize the whole store.
		/// </summary>
		List<Workflow> GetAllWorkflows();

		/// <summary>
		/// Resolves a single workflow by name (latest version when the store is versioned).
		/// The default implementation delegates to <see cref="GetAllWorkflows"/> so existing
		/// in-memory providers keep working without changes.
		/// </summary>
		Task<Workflow?> GetWorkflowAsync(string name, CancellationToken cancellationToken = default)
			=> Task.FromResult(GetAllWorkflows().FirstOrDefault(x => x.WorkflowName == name));
	}
}
