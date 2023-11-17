namespace ActionFlow.Domain.Engine
{
    public class Step
    {
        public Step(string name, string actionType, Dictionary<string, string>? properties = null, string? conditionExpression = null)
        {
            Name = name;
            ActionType = actionType;
            Properties = properties;
            ConditionExpression = conditionExpression;
        }

        public string Name { get; }
        public string ActionType { get; }
        public string? ConditionExpression { get; }
        public Dictionary<string, string>? Properties { get; }
    }
}
