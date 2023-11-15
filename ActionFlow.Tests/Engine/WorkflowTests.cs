using RulesEngine.Models;
using System.Dynamic;

namespace ActionFlow.Tests.Engine
{
    [TestClass]
    public class WorkflowTests
    {
        [TestMethod]
        public void When_running_basic_workflow_it_should_execute()
        {
            //Arrange
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
            var rulesEngine = new RulesEngine.RulesEngine(workflows.ToArray(), null);

            dynamic datas = new ExpandoObject();
            datas.count = 1;
            var inputs = new dynamic[]
              {
                    datas
              };

            //Act
            var resultList = rulesEngine.ExecuteAllRulesAsync("Test Workflow Rule 1", inputs).Result;

            //Assert
            Assert.IsTrue(resultList.TrueForAll(x => x.IsSuccess));
        }
    }
}