﻿using ActionFlow.Actions;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ActionFlow.Engine.Factories;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Tests.Engine
{
    [TestClass]
    public class StepExecutionEvaluatorTests
    {
        [TestMethod]
        public async Task When_evaluating_step_with_a_true_condition_it_should_execute_action()
        {
            //Arrange
            var actionFlowEngine = Substitute.For<IActionFlowEngine>();
            var step = new Step("test", "action", null, "age == 3");
            var executionContext = new ExecutionContext(actionFlowEngine);
            var ageParameter = new Parameter
            {
                Name = "age",
                Expression = "1 + 2"
            };

            executionContext.AddOrUpdateParameter(ageParameter);
            var action = Substitute.For<ActionBase>();
            var stepActionFactory = Substitute.For<IStepActionFactory>();
            stepActionFactory.Get("action").Returns(action);

            var sut = new StepExecutionEvaluator();

            //Act
            await sut.EvaluateAndRunStep(step, executionContext, stepActionFactory);

            //Assert
            stepActionFactory.Received(1).Get("action");
        }

        [TestMethod]
        public async Task When_evaluating_step_with_a_false_condition_it_should_execute_action()
        {
            //Arrange
            var actionFlowEngine = Substitute.For<IActionFlowEngine>();
            var step = new Step("test", "action", null, "age == 3");
            var executionContext = new ExecutionContext(actionFlowEngine);
            var ageParameter = new Parameter
            {
                Name = "age",
                Expression = "2 + 2"
            };

            executionContext.AddOrUpdateParameter(ageParameter);
            var action = Substitute.For<ActionBase>();
            var stepActionFactory = Substitute.For<IStepActionFactory>();
            stepActionFactory.Get("action").Returns(action);

            var sut = new StepExecutionEvaluator();

            //Act
            await sut.EvaluateAndRunStep(step, executionContext, stepActionFactory);

            //Assert
            stepActionFactory.Received(0).Get("action");
        }

        [TestMethod]
        public async Task When_evaluating_step_with_a_step_property_it_should_build_action_properties()
        {
            //Arrange
            var actionFlowEngine = Substitute.For<IActionFlowEngine>();
            var actionProperties = new Dictionary<string, object>
            {
                { "test", true },
                { "testA", "string" },
                { "testB", 1 },
            };
            var step = new Step("test", "action", actionProperties, "age == 3");
            var executionContext = new ExecutionContext(actionFlowEngine);
            var ageParameter = new Parameter
            {
                Name = "age",
                Expression = "1 + 2"
            };

            executionContext.AddOrUpdateParameter(ageParameter);
            var action = Substitute.For<ActionBase>();
            var stepActionFactory = Substitute.For<IStepActionFactory>();
            stepActionFactory.Get("action").Returns(action);

            var sut = new StepExecutionEvaluator();

            //Act
            await sut.EvaluateAndRunStep(step, executionContext, stepActionFactory);

            //Assert
            stepActionFactory.Received(1).Get("action");
        }
    }
}
