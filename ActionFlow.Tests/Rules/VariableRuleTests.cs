using ActionFlow.Engine;
using ActionFlow.Rules;
using NSubstitute;
using RulesEngine.Models;

namespace ActionFlow.Tests.Rules
{
    [TestClass]
    public class VariableRuleTests
    {

        [TestMethod]
        public void When_running_rule_engine_with_basic_workflow_it_should_execute()
        {
            //Arrange
            var workflowProvider = Substitute.For<IWorkflowProvider>();
            workflowProvider.GetAllWorkflows().Returns(CreateBasicWorkflow());

            var resettingsProvider = Substitute.For<IReSettingsProvider>();

            var rulesEngine = new RuleEngineWrapper(workflowProvider, resettingsProvider);

            //Act
            var resultList = rulesEngine.ExecuteAllRulesAsync("Test Workflow Rule 1", new object[] { }).Result;

            //Assert
            Assert.IsTrue(resultList.TrueForAll(x => x.IsSuccess));
            Assert.AreEqual(2, resultList[0].Rule.LocalParams.Count());
        }

        [TestMethod]
        public void When_running_rule_engine_with_false_condition_on_variable_rule_child_it_should_fail()
        {
            //Arrange
            var workflowProvider = Substitute.For<IWorkflowProvider>();
            workflowProvider.GetAllWorkflows().Returns(CreateConditionExpressionWorkflow());

            var resettingsProvider = Substitute.For<IReSettingsProvider>();

            var rulesEngine = new RuleEngineWrapper(workflowProvider, resettingsProvider);

            //Act
            var resultList = rulesEngine.ExecuteAllRulesAsync("Test Workflow Rule 1", new object[] { }).Result;

            //Assert
            Assert.IsFalse(resultList.TrueForAll(x => x.IsSuccess));
            Assert.AreEqual(2, resultList[0].Rule.LocalParams.Count());
        }

        private List<Workflow> CreateBasicWorkflow()
        {
            List<Workflow> workflows = new List<Workflow>();
            Workflow workflow = new Workflow();
            workflow.WorkflowName = "Test Workflow Rule 1";

            List<Rule> rules = new List<Rule>();

            var variableRule1 = new VariableRule();
            variableRule1.Name = "Declare test variables";
            variableRule1.Variables = new Dictionary<string, string>
            {
                { "age", "1" },
                { "canWalk", "false" }
            };
            variableRule1.ConditionExpression = "age == 1 && canWalk == false";

            var variableRule2 = new VariableRule();
            variableRule2.Name = "Test previously declared variables";
            variableRule2.ConditionExpression = "age == 1 && canWalk == false";

            var variableRule3 = new VariableRule();
            variableRule3.Name = "Test previously declared variables 2";
            variableRule3.ConditionExpression = "age == 1 && canWalk == false";

            variableRule1.Rules.Add(variableRule2);
            variableRule1.Rules.Add(variableRule3);

            rules.Add(variableRule1.AsRuleEngineRule());
            workflow.Rules = rules;
            workflows.Add(workflow);

            return workflows;
        }

        private List<Workflow> CreateConditionExpressionWorkflow()
        {
            List<Workflow> workflows = new List<Workflow>();
            Workflow workflow = new Workflow();
            workflow.WorkflowName = "Test Workflow Rule 1";

            List<Rule> rules = new List<Rule>();

            var variableRule1 = new VariableRule();
            variableRule1.Name = "Declare test variables";
            variableRule1.Variables = new Dictionary<string, string>
            {
                { "age", "1" },
                { "canWalk", "false" }
            };

            var variableRule2 = new VariableRule();
            variableRule2.Name = "Test previously declared variables";
            variableRule2.ConditionExpression = "age == 1 && canWalk == true";

            variableRule1.Rules.Add(variableRule2);

            rules.Add(variableRule1.AsRuleEngineRule());
            workflow.Rules = rules;
            workflows.Add(workflow);

            return workflows;
        }
    }
}
