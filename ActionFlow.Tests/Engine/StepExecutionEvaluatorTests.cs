using ActionFlow.Actions;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ActionFlow.Engine.Factories;
using NSubstitute;
using ExecutionContext = ActionFlow.Domain.Engine.ExecutionContext;

namespace ActionFlow.Tests.Engine
{
    [TestClass]
    public class StepExecutionEvaluatorTests
    {
        [TestMethod]
        public void When_evaluating_step_with_a_true_condition_it_should_execute_action()
        {
            //Arrange
            var step = new Step("test", "action", null, "age == 3");
            var executionContext = new ExecutionContext
            {
                Parameters =
                {
                    new Parameter { Name = "age", Expression = "1+2"}
                }
            };

            var action = Substitute.For<ActionBase>();

            var stepActionFactory = Substitute.For<IStepActionFactory>();
            stepActionFactory.Get("action").Returns(action);

            var sut = new StepExecutionEvaluator();

            //Act
            var result = sut.EvaluateAndRunStep(step, executionContext, stepActionFactory);

            //Assert
            stepActionFactory.Received(1);
        }

        [TestMethod]
        public void When_evaluating_step_with_a_false_condition_it_should_execute_action()
        {
            //Arrange
            var step = new Step("test", "action", null, "age == 3");
            var executionContext = new ExecutionContext
            {
                Parameters =
                {
                    new Parameter { Name = "age", Expression = "2+2"}
                }
            };

            var action = Substitute.For<ActionBase>();

            var stepActionFactory = Substitute.For<IStepActionFactory>();
            stepActionFactory.Get("action").Returns(action);

            var sut = new StepExecutionEvaluator();

            //Act
            var result = sut.EvaluateAndRunStep(step, executionContext, stepActionFactory);

            //Assert
            stepActionFactory.Received(0);
        }
    }
}
