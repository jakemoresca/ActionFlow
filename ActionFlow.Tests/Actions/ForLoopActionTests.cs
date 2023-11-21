using ActionFlow.Actions;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActionFlow.Domain.Actions;
using ActionFlow.Domain.Engine;

namespace ActionFlow.Tests.Actions
{
    [TestClass]
    public class ForLoopActionTests
    {
        [TestMethod]
        public async Task When_executing_it_should_repeat_steps_3_times()
        {
            //Arrange
            var stepActionFactory = Substitute.For<IStepActionFactory>();
            stepActionFactory.Get("Variable").Returns(new SetVariableAction());
            var stepExecutionEvaluator = new StepExecutionEvaluator();
            var sut = new ForLoopAction(stepExecutionEvaluator, stepActionFactory);

            var executionContext = new ActionFlow.Engine.ExecutionContext();
            var steps = CreateLoopSteps();
            executionContext.AddOrUpdateParameter("age", 18);
            executionContext.AddOrUpdateActionProperty(ForLoopAction.InitializerVariableKey, "index");
            executionContext.AddOrUpdateActionProperty(ForLoopAction.InitialValueKey, "1");
            executionContext.AddOrUpdateActionProperty(ForLoopAction.LoopConditionKey, "index <= 3");
            executionContext.AddOrUpdateActionProperty(ForLoopAction.IteratorKey, "index + 1");
            executionContext.AddOrUpdateActionProperty(ForLoopAction.StepsKey, steps);
            sut.SetExecutionContext(executionContext);

            //Act
            await sut.ExecuteAction();

            //Assert
            Assert.AreEqual(21, executionContext.EvaluateExpression<int>("age"));
        }

        private static List<Step> CreateLoopSteps()
        {
            var steps = new List<Step>
            {
                new Step("Increment age by 1", "Variable", new Dictionary<string, object>
                {
                    {
                        SetVariableAction.VariablesKey, new Dictionary<string, string>
                        {
                            { "age", "age + 1" }
                        }
                    }
                })
            };

            return steps;
        }
    }
}
