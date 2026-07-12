using ActionFlow.Actions;
using ActionFlow.Engine.Factories;
using ActionFlow.Runner.Registry;
using FluentAssertions;

namespace ActionFlow.Runner.Tests;

[TestClass]
public class ActionRegistryTests
{
    [TestMethod]
    public void Register_instance_makes_it_resolvable_by_action_type()
    {
        var factory = new StepActionFactory([]);
        var registry = new ActionRegistry(factory);

        registry.Register(new SetVariableAction());

        factory.Get("SetVariable").Should().BeOfType<SetVariableAction>();
    }

    [TestMethod]
    public void Unregister_removes_the_action()
    {
        var factory = new StepActionFactory([new SetVariableAction()]);
        var registry = new ActionRegistry(factory);

        registry.Unregister("SetVariable").Should().BeTrue();

        Action get = () => factory.Get("SetVariable");
        get.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    public void RegisterFromAssemblies_scans_and_registers_default_actions()
    {
        var factory = new StepActionFactory([]);
        var registry = new ActionRegistry(factory);

        registry.RegisterFromAssemblies(typeof(SetVariableAction).Assembly);

        // A concrete action with a parameterless constructor is discovered...
        factory.Get("SetVariable").Should().BeOfType<SetVariableAction>();
        factory.Get("ForLoop").Should().BeOfType<ForLoopAction>();
        // ...and each resolution yields a fresh instance (no shared execution context).
        factory.Get("SetVariable").Should().NotBeSameAs(factory.Get("SetVariable"));
    }
}
