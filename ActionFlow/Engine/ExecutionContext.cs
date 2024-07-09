using DynamicExpresso;

namespace ActionFlow.Engine
{
	public class ExecutionContext(IActionFlowEngine actionFlowEngine)
	{
		private readonly Interpreter _intepreter = new Interpreter();
		private readonly IActionFlowEngine _actionFlowEngine = actionFlowEngine;

		private Dictionary<string, object> _actionProperties { get; } = [];

		public IActionFlowEngine GetCurrentEngine() => _actionFlowEngine;

		public void AddOrUpdateParameter(Domain.Engine.Parameter parameter)
		{
			AddOrUpdateParameter(parameter.Name!, _intepreter.Eval(parameter.Expression));
		}

		public void AddOrUpdateParameter(string name, object value)
		{
			_intepreter.SetVariable(name, value);
		}

		public T GetParameter<T>(string key)
		{
			return _intepreter.Eval<T>(key);
		}

		public T EvaluateExpression<T>(string expression)
		{
			return _intepreter.Eval<T>(expression);
		}

		public void AddOrUpdateActionProperty(string key, object value)
		{
			if (_actionProperties.ContainsKey(key))
			{
				_actionProperties[key] = value;
			}
			else
			{
				_actionProperties.Add(key, value);
			}
		}

		public T? GetActionProperty<T>(string key)
		{
			if (_actionProperties.TryGetValue(key, out var value))
			{
				return (T)value;
			}

			return default;
		}

		public void ClearActionProperties()
		{
			foreach (var propertyKey in _actionProperties.Keys)
			{
				_intepreter.UnsetVariable(propertyKey);
			}

			_actionProperties.Clear();
		}
	}
}
