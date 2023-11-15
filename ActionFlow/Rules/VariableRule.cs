using RulesEngine.Models;

namespace ActionFlow.Rules
{
    /// <summary>
    /// Variable Rule is a kind of rule for storing variables which stored values can be used by child rules
    /// </summary>
    public class VariableRule : IRuleWrapper
    {
        public string RuleType => "Variable";
        public string Name { get; set; } = string.Empty;
        public string SuccessEvent { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ConditionExpression { get; set; } = string.Empty;
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
        public List<IRuleWrapper> Rules { get; set; } = new List<IRuleWrapper> { };

        public Rule AsRuleEngineRule()
        {
            Rule rule = new Rule();

            rule.RuleName = $"{RuleType} - {Name}";
            rule.SuccessEvent = SuccessEvent;
            rule.ErrorMessage = ErrorMessage;

            rule.LocalParams = Variables.Select(x =>
            {
                return new ScopedParam { Name = x.Key, Expression = x.Value };
            });

            if (ConditionExpression != string.Empty)
            {
                rule.Expression = ConditionExpression;
                rule.RuleExpressionType = RuleExpressionType.LambdaExpression;
            }

            //if (!rule.LocalParams.Any())
            //{
            //    rule.Expression = ConditionExpression;
            //}

            rule.Rules = Rules.Select(x => x.AsRuleEngineRule()).ToList();

            if (rule.Rules.Any())
            {
                rule.Operator = "And";
            }

            return rule;
        }
    }
}
