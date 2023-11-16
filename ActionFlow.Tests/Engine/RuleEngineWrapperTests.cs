using ActionFlow.Engine;
using NSubstitute;
using RulesEngine.Models;
using System.Dynamic;

namespace ActionFlow.Tests.Engine
{
    [TestClass]
    public class RuleEngineWrapperTests
    {
        [TestMethod]
        public void When_running_rule_engine_with_basic_workflow_it_should_execute()
        {
            //Arrange
            var workflowProvider = Substitute.For<IWorkflowProvider>();
            workflowProvider.GetAllWorkflows().Returns(CreateFakeWorkflows());

            var resettingsProvider = Substitute.For<IReSettingsProvider>();
            resettingsProvider.GetReSettings().Returns(CreateFakeResettings());

            var rulesEngine = new RuleEngineWrapper(workflowProvider, resettingsProvider);

            //Act
            dynamic datas = new ExpandoObject();
            datas.count = 1;
            var inputs = new dynamic[]
              {
                    datas
              };

            var resultList = rulesEngine.ExecuteAllRulesAsync("Test Workflow Rule 1", inputs).Result;

            //Assert
            Assert.IsTrue(resultList.TrueForAll(x => x.IsSuccess));
        }

        private List<Workflow> CreateFakeWorkflows()
        {
            List<Workflow> workflows = new List<Workflow>();
            Workflow workflow = new Workflow();
            workflow.WorkflowName = "Test Workflow Rule 1";

            List<Rule> rules = new List<Rule>();

            Rule rule = new Rule();
            rule.RuleName = "Test Rule";
            rule.SuccessEvent = "Count is within tolerance.";
            rule.ErrorMessage = "Over expected.";
            rule.Expression = "count < 3";
            rule.RuleExpressionType = RuleExpressionType.LambdaExpression;

            rules.Add(rule);
            workflow.Rules = rules;
            workflows.Add(workflow);

            return workflows;
        }

        private ReSettings CreateFakeResettings()
        {
            var reSettingsWithCustomTypes = new ReSettings();
            return reSettingsWithCustomTypes;
        }
    }
}
