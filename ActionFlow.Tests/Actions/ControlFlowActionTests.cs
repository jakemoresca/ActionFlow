using ActionFlow.Actions;
using ActionFlow.Domain.Actions;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ActionFlow.Engine.Factories;
using NSubstitute;

namespace ActionFlow.Tests.Actions
{
    [TestClass]
    public class ControlFlowActionTests : ActionBaseTests
    {
        [TestMethod]
        public async Task When_executing_it_should_call_only_valid_scoped_workflow_steps()
        {
            //Arrange
            var sut = new ControlFlowAction();
            var stepExecutionEvaluator = new StepExecutionEvaluator();
            ActionFlowEngine.GetStepExecutionEvaluator().Returns(stepExecutionEvaluator);
            var executionContext = ExecutionContext;

            var conditions = CreateScopedWorkflows();
            executionContext.AddOrUpdateParameter("age", 18);
            executionContext.AddOrUpdateActionProperty(ControlFlowAction.ConditionsKey, conditions);
            sut.SetExecutionContext(executionContext);

            //Act
            await sut.ExecuteAction();

            //Assert
            Assert.AreEqual(18, executionContext.EvaluateExpression<int>("age"));
            Assert.AreEqual(true, executionContext.EvaluateExpression<bool>("canWalk"));
        }

        [TestMethod]
        public async Task When_executing_it_should_call_all_scoped_workflow_steps()
        {
            //Arrange
            var sut = new ControlFlowAction();
            var stepExecutionEvaluator = new StepExecutionEvaluator();
            ActionFlowEngine.GetStepExecutionEvaluator().Returns(stepExecutionEvaluator);
            var executionContext = ExecutionContext;

            var conditions = CreateScopedWorkflowsThatAllWillBeCalled();
            executionContext.AddOrUpdateParameter("age", 1);
            executionContext.AddOrUpdateActionProperty(ControlFlowAction.ConditionsKey, conditions);
            sut.SetExecutionContext(executionContext);

            //Act
            await sut.ExecuteAction();

            //Assert
            Assert.AreEqual(true, executionContext.EvaluateExpression<bool>("isBaby"));
            Assert.AreEqual(18, executionContext.EvaluateExpression<int>("age"));
            Assert.AreEqual(true, executionContext.EvaluateExpression<bool>("canWalk"));
        }

        private static List<ScopedWorkflow> CreateScopedWorkflows()
        {
            var conditions = new List<ScopedWorkflow>
            {
                new ScopedWorkflow
                {
                    Expression = "age == 1",
                    Steps = new List<Step>
                    {
                        new Step("Set canWalk to false", "Variable", new Dictionary<string, object>
                        {
                            {
                                SetVariableAction.VariablesKey, new Dictionary<string, string>
                                {
                                    { "canWalk", "false" }
                                }
                            }
                        })
                    }
                },
                new ScopedWorkflow
                {
                    Expression = "age == 18",
                    Steps = new List<Step>
                    {
                        new Step("Set canWalk to true", "Variable", new Dictionary<string, object>
                        {
                            {
                                SetVariableAction.VariablesKey, new Dictionary<string, string>
                                {
                                    { "canWalk", "true" }
                                }
                            }
                        })
                    }
                }
            };

            return conditions;
        }

        private static List<ScopedWorkflow> CreateScopedWorkflowsThatAllWillBeCalled()
        {
            var conditions = new List<ScopedWorkflow>
            {
                new ScopedWorkflow
                {
                    Expression = "age == 1",
                    Steps = new List<Step>
                    {
                        new Step("Set canWalk to false", "Variable", new Dictionary<string, object>
                        {
                            {
                                SetVariableAction.VariablesKey, new Dictionary<string, string>
                                {
                                    { "canWalk", "false" },
                                    { "isBaby", "true" },
                                    { "age", "18" }
                                }
                            }
                        })
                    }
                },
                new ScopedWorkflow
                {
                    Expression = "age == 18",
                    Steps = new List<Step>
                    {
                        new Step("Set canWalk to true", "Variable", new Dictionary<string, object>
                        {
                            {
                                SetVariableAction.VariablesKey, new Dictionary<string, string>
                                {
                                    { "canWalk", "true" }
                                }
                            }
                        })
                    }
                }
            };

            return conditions;
        }
    }
}
