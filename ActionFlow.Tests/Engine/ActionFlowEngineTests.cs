using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine.Providers;
using NSubstitute;

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
    }
}
