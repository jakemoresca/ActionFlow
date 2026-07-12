using System.Reflection;
using ActionFlow.Actions;
using ActionFlow.Engine.Factories;

namespace ActionFlow.Runner.Registry;

/// <inheritdoc />
public class ActionRegistry(IStepActionFactory factory) : IActionRegistry
{
    public IStepActionFactory Factory => factory;

    public IActionRegistry Register(IActionBase action)
    {
        factory.AddOrUpdate(action.ActionType, () => action);
        return this;
    }

    public IActionRegistry Register(string actionType, Func<IActionBase> actionFactory)
    {
        factory.AddOrUpdate(actionType, actionFactory);
        return this;
    }

    public IActionRegistry RegisterFromAssemblies(params Assembly[] assemblies)
    {
        var actionTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IActionBase).IsAssignableFrom(type)
                           && type is { IsAbstract: false, IsInterface: false }
                           && type.GetConstructor(Type.EmptyTypes) is not null);

        foreach (var type in actionTypes)
        {
            // Resolve the ActionType key once from a probe instance, then register a fresh
            // instance per resolution so actions never share mutable execution context.
            var probe = (IActionBase)Activator.CreateInstance(type)!;
            Register(probe.ActionType, () => (IActionBase)Activator.CreateInstance(type)!);
        }

        return this;
    }

    public bool Unregister(string actionType) => factory.Remove(actionType);
}
