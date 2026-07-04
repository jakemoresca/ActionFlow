using ActionFlow.Contracts;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ActionFlow.Runner.Observers;
using FluentAssertions;
using NSubstitute;
using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Runner.Tests;

[TestClass]
public class BufferingStepObserverTests
{
    [TestMethod]
    public async Task OnStepCompleted_buffers_a_StepCompleted_event_tagged_with_execution_and_index()
    {
        var buffer = new ExecutionEventBuffer();
        var executionId = Guid.NewGuid();
        var context = new ExecutionContext(Substitute.For<IActionFlowEngine>())
        {
            ExecutionId = executionId,
            CurrentStepIndex = 2
        };

        await new BufferingStepObserver(buffer)
            .OnStepCompletedAsync(new Step("check", "Variable"), context, TimeSpan.FromMilliseconds(12));

        buffer.Events.Should().ContainSingle()
            .Which.Should().BeOfType<StepCompleted>()
            .Which.Should().BeEquivalentTo(new { ExecutionId = executionId, StepName = "check", Index = 2 });
    }

    [TestMethod]
    public async Task OnStepFailed_buffers_a_StepFailed_event()
    {
        var buffer = new ExecutionEventBuffer();
        var executionId = Guid.NewGuid();
        var context = new ExecutionContext(Substitute.For<IActionFlowEngine>()) { ExecutionId = executionId, CurrentStepIndex = 1 };

        await new BufferingStepObserver(buffer)
            .OnStepFailedAsync(new Step("boom", "Variable"), context, new InvalidOperationException("nope"));

        buffer.Events.Should().ContainSingle()
            .Which.Should().BeOfType<StepFailed>()
            .Which.Should().BeEquivalentTo(new { ExecutionId = executionId, StepName = "boom", Error = "nope", Index = 1 });
    }

    [TestMethod]
    public async Task Nothing_is_buffered_when_the_context_has_no_execution_id()
    {
        var buffer = new ExecutionEventBuffer();
        var context = new ExecutionContext(Substitute.For<IActionFlowEngine>()); // ExecutionId == null

        await new BufferingStepObserver(buffer)
            .OnStepCompletedAsync(new Step("check", "Variable"), context, TimeSpan.Zero);

        buffer.Events.Should().BeEmpty();
    }
}
