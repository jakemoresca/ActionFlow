using RulesEngine.Models;

namespace ActionFlow.Rules
{
    /// <summary>
    /// Variable Rule is a kind of rule for storing variables which stored values can be used by child rules
    /// </summary>
    public class VariableRule : RuleWrapper
    {
        public override string RuleType => "Variable";
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

        public override Rule AsRuleEngineRule()
        {
            var rule = base.AsRuleEngineRule();

            rule.LocalParams = Variables.Select(x =>
            {
                return new ScopedParam { Name = x.Key, Expression = x.Value };
            });

            return rule;
        }
    }
}
