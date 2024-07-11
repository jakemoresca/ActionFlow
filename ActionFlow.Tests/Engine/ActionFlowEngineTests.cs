using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine.Providers;
using NSubstitute;
using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Tests.Engine
{
    [TestClass]
    public class ActionFlowEngineTests
    {
        [TestMethod]
        public async Task When_running_rule_engine_with_basic_workflow_it_should_execute()
        {
            //Arrange
            var workflowProvider = Substitute.For<IWorkflowProvider>();
            var workflows = CreateFakeWorkflows();
            workflowProvider.GetAllWorkflows().Returns(workflows);

            var stepActionFactory = Substitute.For<IStepActionFactory>();
            var stepExecutionEvaluator = Substitute.For<IStepExecutionEvaluator>();

            var actionFlowEngine = new ActionFlowEngine(workflowProvider, stepActionFactory, stepExecutionEvaluator);

            var inputs = new Parameter[]
            {
                new() { Name = "test", Expression = "true"}
            };

            //Act
            await actionFlowEngine.ExecuteWorkflowAsync("Test Workflow Rule 1", inputs);

            //Assert
            await stepExecutionEvaluator.ReceivedWithAnyArgs(2).EvaluateAndRunStep(workflows[0].Steps[0], new ExecutionContext(actionFlowEngine), stepActionFactory);
        }

        [TestMethod]
        public async Task When_running_rule_engine_with_output_parameter_workflow_it_evaluate_output()
        {
            //Arrange
            var workflowProvider = Substitute.For<IWorkflowProvider>();
            var workflows = CreateFakeWorkflowsWithOutput();
            workflowProvider.GetAllWorkflows().Returns(workflows);

            var stepActionFactory = Substitute.For<IStepActionFactory>();
            var stepExecutionEvaluator = Substitute.For<IStepExecutionEvaluator>();
            var actionFlowEngine = new ActionFlowEngine(workflowProvider, stepActionFactory, stepExecutionEvaluator);

            var executionContext = new ExecutionContext(actionFlowEngine);
            executionContext.AddOrUpdateParameter(new Parameter { Name = "age", Expression = "1" });
            executionContext.AddOrUpdateParameter(new Parameter { Name = "canWalk", Expression = "true" });

            stepExecutionEvaluator.EvaluateAndRunStep(workflows[0].Steps[0], executionContext, stepActionFactory).ReturnsForAnyArgs(executionContext);

            //Act
            var output = await actionFlowEngine.ExecuteWorkflowAsync("Test Workflow Rule 1", executionContext);

            //Assert
            await stepExecutionEvaluator.ReceivedWithAnyArgs(1).EvaluateAndRunStep(workflows[0].Steps[0], new ExecutionContext(actionFlowEngine), stepActionFactory);
            Assert.AreEqual(2 ,output.OutputParameters.Count);
            Assert.AreEqual(false, output.OutputParameters["canVote"]);
            Assert.AreEqual(1, output.OutputParameters["age"]);
        }

        private static List<Workflow> CreateFakeWorkflows()
        {
            List<Workflow> workflows = [];
            var steps = new List<Step>
            {
                new("initialize", "Variable", new Dictionary<string, object>
                {
                    { "age", "1" },
                    { "canWalk", "true" },
                }),
                new("test variable value", "Variable", [], "age == 1 && canWalk == true")
            };

            Workflow workflow = new Workflow("Test Workflow Rule 1", steps);
            workflows.Add(workflow);

            return workflows;
        }

        private static List<Workflow> CreateFakeWorkflowsWithOutput()
        {
            List<Workflow> workflows = [];
            var steps = new List<Step>
            {
                new("test variable value", "Variable", [], "age == 1 && canWalk == true")
            };

            Workflow workflow = new Workflow("Test Workflow Rule 1", steps,
			[
				new() { Name = "canVote", Expression = "age >= 18 && canWalk == true"},
                new() { Name = "age", Expression = "age"}
            ]);
            workflows.Add(workflow);

            return workflows;
        }
    }
}
