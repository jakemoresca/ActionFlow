using ActionFlow.Actions;

namespace ActionFlow.Engine.Factories
{
    public interface IStepActionFactory
    {
        void AddOrUpdate(string actionName, Func<ActionBase> action);
        void Clear();
        ActionBase Get(string name);
        bool Remove(string name);
    }
}