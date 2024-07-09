namespace ActionFlow.Domain.Engine
{
    public class Step(string name, string actionType, Dictionary<string, object>? properties = null, string? conditionExpression = null)
	{
		public string Name { get; } = name;
		public string ActionType { get; } = actionType;
		public string? ConditionExpression { get; } = conditionExpression;
		public Dictionary<string, object>? Properties { get; } = properties ?? [];
	}
}
