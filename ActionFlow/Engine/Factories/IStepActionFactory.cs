using ActionFlow.Actions;

namespace ActionFlow.Engine.Factories
{
    public interface IStepActionFactory
    {
        bool AddOrUpdate(string actionName, Func<IActionBase> action);
        bool Clear();
        IActionBase Get(string name);
        bool Remove(string name);
    }
}