using ActionFlow.Actions;

namespace ActionFlow.Engine.Factories
{
    public interface IStepActionFactory
    {
        bool AddOrUpdate(string actionName, Func<ActionBase> action);
        bool Clear();
        ActionBase Get(string name);
        bool Remove(string name);
    }
}