using RulesEngine.Models;

namespace ActionFlow.Rules
{
    public abstract class RuleWrapper : IRuleWrapper
    {
        public virtual string RuleType { get; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SuccessEvent { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ConditionExpression { get; set; } = string.Empty;
        public List<IRuleWrapper> Rules { get; set; } = new List<IRuleWrapper> { };

        public virtual Rule AsRuleEngineRule()
        {
            var rule = new Rule();

            rule.RuleName = $"{RuleType} - {Name}";
            rule.SuccessEvent = SuccessEvent;
            rule.ErrorMessage = ErrorMessage;

            if (ConditionExpression != string.Empty)
            {
                rule.Expression = ConditionExpression;
                rule.RuleExpressionType = RuleExpressionType.LambdaExpression;
            }

            rule.Rules = Rules.Select(x => x.AsRuleEngineRule()).ToList();

            if (rule.Rules.Any())
            {
                rule.Operator = "And";
            }

            return rule;
        }
    }
}
