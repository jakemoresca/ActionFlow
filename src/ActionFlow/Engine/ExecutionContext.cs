using DynamicExpresso;

namespace ActionFlow.Engine
{
    public class ExecutionContext
    {
        private readonly Interpreter _intepreter;
        private readonly IActionFlowEngine _actionFlowEngine;

        public ExecutionContext(IActionFlowEngine actionFlowEngine)
        {
            _actionProperties = [];
            _intepreter = new Interpreter();
            _actionFlowEngine = actionFlowEngine;
        }

        private Dictionary<string, object> _actionProperties { get; }

        // Tracks every persistent parameter set on the context (inputs + action
        // outputs) so a workflow can opt into emitting the whole final context.
        private readonly Dictionary<string, object> _parameters = [];

        /// <summary>
        /// Correlation id for the running execution. Set by a host (e.g. ActionFlow.Runner) so the
        /// step observer can tag emitted events. Null for standalone/library executions.
        /// </summary>
        public Guid? ExecutionId { get; set; }

        /// <summary>Zero-based index of the step currently being evaluated; -1 before the first step.</summary>
        public int CurrentStepIndex { get; set; } = -1;

        public IActionFlowEngine GetCurrentEngine() => _actionFlowEngine;

        public void AddOrUpdateParameter(Domain.Engine.Parameter parameter)
        {
            AddOrUpdateParameter(parameter.Name!, _intepreter.Eval(parameter.Expression));
        }

        public void AddOrUpdateParameter(string name, object value)
        {
            _intepreter.SetVariable(name, value);
            _parameters[name] = value;
        }

        /// <summary>
        /// All persistent parameters currently set on the context. Used when a
        /// workflow opts into emitting the entire final context as output.
        /// </summary>
        public IReadOnlyDictionary<string, object> GetAllParameters() => _parameters;

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
