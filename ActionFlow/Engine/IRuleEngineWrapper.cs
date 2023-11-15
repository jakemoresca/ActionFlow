using RulesEngine.Models;

namespace ActionFlow.Engine
{
    public interface IRuleEngineWrapper
    {
        ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, params object[] inputs);
    }
}