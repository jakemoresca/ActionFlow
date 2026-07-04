using ActionFlow.Actions;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine.Observers;
using NSubstitute;
using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Tests.Engine
{
    [TestClass]
    public class StepExecutionObserverTests
    {
        [TestMethod]
        public async Task When_step_runs_it_notifies_starting_then_completed()
        {
            var observer = Substitute.For<IStepExecutionObserver>();
            var (step, executionContext, stepActionFactory) = ArrangeExecutableStep();

            var sut = new StepExecutionEvaluator(observer);

            await sut.EvaluateAndRunStep(step, executionContext, stepActionFactory);

            await observer.Received(1).OnStepStartingAsync(step, executionContext, Arg.Any<CancellationToken>());
            await observer.Received(1).OnStepCompletedAsync(step, executionContext, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
            await observer.DidNotReceiveWithAnyArgs().OnStepFailedAsync(default!, default!, default!, default);
        }

        [TestMethod]
        public async Task When_step_is_skipped_it_notifies_nothing()
        {
            var observer = Substitute.For<IStepExecutionObserver>();
            var actionFlowEngine = Substitute.For<IActionFlowEngine>();
            var executionContext = new ExecutionContext(actionFlowEngine);
            executionContext.AddOrUpdateParameter(new Parameter { Name = "age", Expression = "2" });
            var step = new Step("test", "action", null, "age == 3"); // false → skipped
            var stepActionFactory = Substitute.For<IStepActionFactory>();

            var sut = new StepExecutionEvaluator(observer);

            await sut.EvaluateAndRunStep(step, executionContext, stepActionFactory);

            await observer.DidNotReceiveWithAnyArgs().OnStepStartingAsync(default!, default!, default);
            await observer.DidNotReceiveWithAnyArgs().OnStepCompletedAsync(default!, default!, default, default);
        }

        [TestMethod]
        public async Task When_action_throws_it_notifies_failed_and_rethrows()
        {
            var observer = Substitute.For<IStepExecutionObserver>();
            var actionFlowEngine = Substitute.For<IActionFlowEngine>();
            var executionContext = new ExecutionContext(actionFlowEngine);
            var step = new Step("test", "action");

            var failing = Substitute.For<ActionBase>();
            failing.ExecuteAction().Returns<Task>(_ => throw new InvalidOperationException("boom"));
            var stepActionFactory = Substitute.For<IStepActionFactory>();
            stepActionFactory.Get("action").Returns(failing);

            var sut = new StepExecutionEvaluator(observer);

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(
                () => sut.EvaluateAndRunStep(step, executionContext, stepActionFactory));

            await observer.Received(1).OnStepFailedAsync(step, executionContext, Arg.Any<InvalidOperationException>(), Arg.Any<CancellationToken>());
            await observer.DidNotReceiveWithAnyArgs().OnStepCompletedAsync(default!, default!, default, default);
        }

        private static (Step, ExecutionContext, IStepActionFactory) ArrangeExecutableStep()
        {
            var actionFlowEngine = Substitute.For<IActionFlowEngine>();
            var executionContext = new ExecutionContext(actionFlowEngine);
            var step = new Step("test", "action");
            var action = Substitute.For<ActionBase>();
            var stepActionFactory = Substitute.For<IStepActionFactory>();
            stepActionFactory.Get("action").Returns(action);

            return (step, executionContext, stepActionFactory);
        }
    }
}
