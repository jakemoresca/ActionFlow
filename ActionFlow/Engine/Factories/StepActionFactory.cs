using ActionFlow.Actions;

namespace ActionFlow.Engine.Factories
{
	public class StepActionFactory(IEnumerable<IActionBase> defaultActions) : IStepActionFactory
	{
		private readonly Dictionary<string, Func<IActionBase>> _actionRegistry = defaultActions.Select(x =>
			{
				return new KeyValuePair<string, Func<IActionBase>>(x.ActionType, () => x);
			}).ToDictionary(x => x.Key, x => x.Value);

		public bool AddOrUpdate(string actionName, Func<IActionBase> action)
		{
			if (!_actionRegistry.TryAdd(actionName, action))
			{
				_actionRegistry[actionName] = action;
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
			if (_actionRegistry.TryGetValue(name, out var value))
			{
				return value();
			}
			throw new KeyNotFoundException($"Action with name: {name} does not exist");
		}
	}
}
