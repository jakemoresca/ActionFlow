using ActionFlow.Actions;

namespace ActionFlow.Engine.Factories
{
    public class StepActionFactory : IStepActionFactory
    {
        private readonly Dictionary<string, Func<ActionBase>> _actionRegistry;

        public StepActionFactory()
        {
            _actionRegistry = new Dictionary<string, Func<ActionBase>>();
        }

        public void AddOrUpdate(string actionName, Func<ActionBase> action)
        {
            if (_actionRegistry.ContainsKey(actionName))
            {
                _actionRegistry[actionName] = action;
            }
            else
            {
                _actionRegistry.Add(actionName, action);
            }
        }

        public bool Remove(string name)
        {
            return _actionRegistry.Remove(name);
        }

        public void Clear()
        {
            _actionRegistry.Clear();
        }

        public ActionBase Get(string name)
        {
            if (_actionRegistry.ContainsKey(name))
            {
                return _actionRegistry[name]();
            }
            throw new KeyNotFoundException($"Action with name: {name} does not exist");
        }
    }
}
