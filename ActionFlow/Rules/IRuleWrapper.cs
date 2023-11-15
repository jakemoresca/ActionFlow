using RulesEngine.Models;

namespace ActionFlow.Rules
{
    public interface IRuleWrapper
    {
        string ConditionExpression { get; set; }
        string ErrorMessage { get; set; }
        string Name { get; set; }
        string RuleType { get; }
        string SuccessEvent { get; set; }
        Dictionary<string, string> Variables { get; set; }

        Rule AsRuleEngineRule();
    }
}