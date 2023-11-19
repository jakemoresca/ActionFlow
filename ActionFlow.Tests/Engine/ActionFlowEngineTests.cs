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
            workflowProvider.GetAllWorkflows().Returns(CreateFakeWorkflows());

            var stepActionFactory = Substitute.For<IStepActionFactory>();
            var stepExecutionEvaluator = Substitute.For<IStepExecutionEvaluator>();

            var rulesEngine = new ActionFlowEngine(workflowProvider, stepActionFactory, stepExecutionEvaluator);

            var inputs = new Parameter[]
            {
                new Parameter { Name = "test", Expression = "true"}
            };

            //Act
            await rulesEngine.ExecuteWorkflowAsync("Test Workflow Rule 1", inputs);

            //Assert
            stepExecutionEvaluator.Received(1);
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

            var executionContext = new ExecutionContext();
            executionContext.AddOrUpdateParameter(new Parameter { Name = "age", Expression = "1" });
            executionContext.AddOrUpdateParameter(new Parameter { Name = "canWalk", Expression = "true" });

            stepExecutionEvaluator.EvaluateAndRunStep(workflows[0].Steps[0], executionContext, stepActionFactory).ReturnsForAnyArgs(executionContext);
            var rulesEngine = new ActionFlowEngine(workflowProvider, stepActionFactory, stepExecutionEvaluator);

            //Act
            var output = await rulesEngine.ExecuteWorkflowAsync("Test Workflow Rule 1", executionContext);

            //Assert
            stepExecutionEvaluator.Received(1);
            Assert.AreEqual(output.OutputParameters.Count, 2);
            Assert.AreEqual(output.OutputParameters["canVote"], false);
            Assert.AreEqual(output.OutputParameters["age"], 1);
        }

        private List<Workflow> CreateFakeWorkflows()
        {
            List<Workflow> workflows = new List<Workflow>();
            var steps = new List<Step>
            {
                new Step("initialize", "Variable", new Dictionary<string, object>
                {
                    { "age", "1" },
                    { "canWalk", "true" },
                }),
                new Step("test variable value", "Variable", new Dictionary<string, object>(), "age == 1 && canWalk == true")
            };

            Workflow workflow = new Workflow("Test Workflow Rule 1", steps);
            workflows.Add(workflow);

            return workflows;
        }

        private List<Workflow> CreateFakeWorkflowsWithOutput()
        {
            List<Workflow> workflows = new List<Workflow>();
            var steps = new List<Step>
            {
                new Step("test variable value", "Variable", new Dictionary<string, object>(), "age == 1 && canWalk == true")
            };

            Workflow workflow = new Workflow("Test Workflow Rule 1", steps, new List<Parameter>
            {
                new Parameter{ Name = "canVote", Expression = "age >= 18 && canWalk == true"},
                new Parameter{ Name = "age", Expression = "age"}
            });
            workflows.Add(workflow);

            return workflows;
        }
    }
}
