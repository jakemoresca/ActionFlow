using RulesEngine.Models;

namespace ActionFlow.Engine
{
    public interface IWorkflowProvider
    {
        List<Workflow> GetAllWorkflows();
    }
}