using ActionFlow.Actions;

namespace ActionFlow.Engine.Factories
{
    public class StepActionFactory : IStepActionFactory
    {
        private readonly Dictionary<string, Func<IActionBase>> _actionRegistry;

        public StepActionFactory(IEnumerable<IActionBase> defaultActions)
        {
            _actionRegistry = defaultActions.Select(x =>
            {
                return new KeyValuePair<string, Func<IActionBase>>(x.ActionType, () => x);
            }).ToDictionary(x => x.Key, x => x.Value);
        }

        public bool AddOrUpdate(string actionName, Func<IActionBase> action)
        {
            if (_actionRegistry.ContainsKey(actionName))
            {
                _actionRegistry[actionName] = action;
            }
            else
            {
                _actionRegistry.Add(actionName, action);
            }

            return true;
        }

        public bool Remove(string name)
        {
            return _actionRegistry.Remove(name);
        }

        public bool Clear()
        {
            _actionRegistry.Clear();
            return true;
        }

        public IActionBase Get(string name)
        {
            if (_actionRegistry.ContainsKey(name))
            {
                return _actionRegistry[name]();
            }
            throw new KeyNotFoundException($"Action with name: {name} does not exist");
        }
    }
}
