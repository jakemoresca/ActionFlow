using System.Reflection;
using ActionFlow.Actions;
using ActionFlow.Engine.Factories;

namespace ActionFlow.Runner.Registry;

/// <summary>
/// The ReadMe's "ActionRegistry — a way for registering actions (with or without the default ones)".
/// Wraps the core <see cref="IStepActionFactory"/> so a host can add custom actions at runtime or by
/// scanning assemblies, keeping registration concerns out of the engine.
/// </summary>
public interface IActionRegistry
{
    /// <summary>The underlying factory the engine resolves actions from.</summary>
    IStepActionFactory Factory { get; }

    /// <summary>Registers (or replaces) an action instance, keyed by its <see cref="IActionBase.ActionType"/>.</summary>
    IActionRegistry Register(IActionBase action);

    /// <summary>Registers (or replaces) an action by type key with a factory delegate.</summary>
    IActionRegistry Register(string actionType, Func<IActionBase> factory);

    /// <summary>Scans assemblies for concrete <see cref="IActionBase"/> types with a public parameterless constructor and registers them.</summary>
    IActionRegistry RegisterFromAssemblies(params Assembly[] assemblies);

    /// <summary>Removes an action by its type key.</summary>
    bool Unregister(string actionType);
}
