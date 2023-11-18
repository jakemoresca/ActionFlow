namespace ActionFlow.Domain.Engine
{
    public class Step
    {
        public Step(string name, string actionType, Dictionary<string, object>? properties = null, string? conditionExpression = null)
        {
            Name = name;
            ActionType = actionType;
            Properties = properties ?? new Dictionary<string, object>();
            ConditionExpression = conditionExpression;
        }

        public string Name { get; }
        public string ActionType { get; }
        public string? ConditionExpression { get; }
        public Dictionary<string, object>? Properties { get; }
    }
}
