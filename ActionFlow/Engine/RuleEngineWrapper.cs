using RulesEngine.Interfaces;
using RulesEngine.Models;

namespace ActionFlow.Engine
{
    public class RuleEngineWrapper : IRuleEngineWrapper
    {
        private readonly IRulesEngine _rulesEngine;

        public RuleEngineWrapper(IWorkflowProvider workflowProvider, IReSettingsProvider reSettingsProvider)
        {
            _rulesEngine = new RulesEngine.RulesEngine(workflowProvider.GetAllWorkflows().ToArray(), reSettingsProvider.GetReSettings());
        }

        public ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, params object[] inputs)
        {
            return _rulesEngine.ExecuteAllRulesAsync(workflowName, inputs);
        }
    }
}
