using ActionFlow.Actions;
using ActionFlow.Domain.Actions;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine.Providers;
using NSubstitute;

namespace ActionFlow.Tests.Actions
{
    [TestClass]
    public class CallWorkflowActionTests
    {
        [TestMethod]
        public async Task When_executing_it_should_call_other_workflow_and_get_output_variable()
        {
            //Arrange
            var workflowProvider = Substitute.For<IWorkflowProvider>();
            var workflows = CreateFakeWorkflowsWithOutput();
            workflowProvider.GetAllWorkflows().Returns(workflows);

            var stepActionFactory = Substitute.For<IStepActionFactory>();
            stepActionFactory.Get("Variable").Returns(new SetVariableAction());

            var stepExecutionEvaluator = new StepExecutionEvaluator();
            var actionFlowEngine = new ActionFlowEngine(workflowProvider, stepActionFactory, stepExecutionEvaluator);

            var sut = new CallWorkflowAction();
            var executionContext = new ActionFlow.Engine.ExecutionContext(actionFlowEngine);
            var parameters = new Dictionary<string, string>
            {
                { "age", "18" }
            };
            var targetWorkflowName = "Test Workflow Rule 2";
            var resultVariableName = "result";
            executionContext.AddOrUpdateActionProperty(CallWorkflowAction.WorkflowNameKey, targetWorkflowName);
            executionContext.AddOrUpdateActionProperty(CallWorkflowAction.ResultVariableKey, resultVariableName);
            executionContext.AddOrUpdateActionProperty(CallWorkflowAction.ParametersKey, parameters);
            sut.SetExecutionContext(executionContext);

            //Act
            await sut.ExecuteAction();

            //Assert
            var output = executionContext.EvaluateExpression<Dictionary<string, object>>(resultVariableName);
            Assert.AreEqual(true, output["canVote"]);
        }

        private static List<Workflow> CreateFakeWorkflowsWithOutput()
        {
            List<Workflow> workflows = [];

            var targetWorkflowName = "Test Workflow Rule 2";

            var steps2 = new List<Step>
            {
                new("test variable value", "Variable", new Dictionary<string, object>
                {
                    { 
                        SetVariableAction.VariablesKey, new Dictionary<string, string>
                        {
                            { "canVote", "age >= 18" }
                        }
                    }
                })
            };

            Workflow workflow2 = new Workflow(targetWorkflowName, steps2,
			[
				new() { Name = "canVote", Expression = "canVote"}
            ]);
            workflows.Add(workflow2);

            return workflows;
        }
    }
}
