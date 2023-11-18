using DynamicExpresso;

namespace ActionFlow.Engine
{
    public class ExecutionContext
    {
        private Interpreter _intepreter;

        public ExecutionContext()
        {
            _actionProperties = new Dictionary<string, object>();
            _intepreter = new Interpreter();
        }

        private Dictionary<string, object> _actionProperties { get; }

        public void AddOrUpdateParameter(Domain.Engine.Parameter parameter)
        {
            _intepreter.SetVariable(parameter.Name, _intepreter.Eval(parameter.Expression));
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

        public T GetActionProperty<T>(string key)
        {
            if (_actionProperties.TryGetValue(key, out var value))
            {
                return (T)value;
            }

            throw new InvalidOperationException($"No action property with key: {key}");
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
