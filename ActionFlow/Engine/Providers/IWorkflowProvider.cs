using ActionFlow.Domain.Engine;

namespace ActionFlow.Engine.Providers
{
    public interface IWorkflowProvider
    {
        List<Workflow> GetAllWorkflows();
    }
}