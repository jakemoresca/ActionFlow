using ActionFlow.Engine;
using ActionFlow.Rules;
using NSubstitute;
using RulesEngine.Models;

namespace ActionFlow.Tests.Rules
{
    [TestClass]
    public class ApiCallRuleTests
    {
        [TestMethod]
        public void When_running_api_call_rule_with_get_method_it_should_execute()
        {
            //Arrange
            var workflowProvider = Substitute.For<IWorkflowProvider>();
            workflowProvider.GetAllWorkflows().Returns(CreateGetApiCallRuleWorkflow());

            var rulesEngine = new RuleEngineWrapper(workflowProvider, new ReSettingsProvider());

            //Act
            var resultList = rulesEngine.ExecuteAllRulesAsync("Test Workflow Rule 1", new object[] { }).Result;

            //Assert
            Assert.IsTrue(resultList.TrueForAll(x => x.IsSuccess));
        }

        [TestMethod]
        public void When_running_api_call_rule_with_post_method_it_should_execute()
        {
            //Arrange
            var workflowProvider = Substitute.For<IWorkflowProvider>();
            workflowProvider.GetAllWorkflows().Returns(CreatePostApiCallRuleWorkflow());

            var rulesEngine = new RuleEngineWrapper(workflowProvider, new ReSettingsProvider());

            //Act
            var resultList = rulesEngine.ExecuteAllRulesAsync("Test Workflow Rule 1", new object[] { }).Result;

            //Assert
            Assert.IsTrue(resultList.TrueForAll(x => x.IsSuccess));
        }

        [TestMethod]
        public void When_running_api_call_rule_with_headers_it_should_execute_and_return_200_status_code()
        {
            //Arrange
            var workflowProvider = Substitute.For<IWorkflowProvider>();
            workflowProvider.GetAllWorkflows().Returns(CreateApiCallRuleWithHeadersWorkflow());

            var rulesEngine = new RuleEngineWrapper(workflowProvider, new ReSettingsProvider());

            //Act
            var resultList = rulesEngine.ExecuteAllRulesAsync("Test Workflow Rule 1", new object[] { }).Result;

            //Assert
            Assert.IsTrue(resultList.TrueForAll(x => x.IsSuccess));
        }

        private List<Workflow> CreateGetApiCallRuleWorkflow()
        {
            List<Workflow> workflows = new List<Workflow>();
            Workflow workflow = new Workflow();
            workflow.WorkflowName = "Test Workflow Rule 1";

            List<Rule> rules = new List<Rule>();

            var apiCallRule = new ApiCallRule();
            apiCallRule.Name = "Call test api";
            apiCallRule.Url = "http://httpbin.org/get";
            apiCallRule.ReturnVariableName = "apiResult";

            var variableRule2 = new VariableRule();
            variableRule2.Name = "Test api call result";
            variableRule2.ConditionExpression = "apiResult.Body != null";

            apiCallRule.Rules.Add(variableRule2);

            rules.Add(apiCallRule.AsRuleEngineRule());
            workflow.Rules = rules;
            workflows.Add(workflow);

            return workflows;
        }

        private List<Workflow> CreateApiCallRuleWithHeadersWorkflow()
        {
            List<Workflow> workflows = new List<Workflow>();
            Workflow workflow = new Workflow();
            workflow.WorkflowName = "Test Workflow Rule 1";

            List<Rule> rules = new List<Rule>();

            var apiCallRule = new ApiCallRule();
            apiCallRule.Name = "Call test api";
            apiCallRule.Url = "http://httpbin.org/get";
            apiCallRule.Headers = new Dictionary<string, string>
            {
                { "test", "value" }
            };
            apiCallRule.ReturnVariableName = "apiResult";

            var variableRule2 = new VariableRule();
            variableRule2.Name = "Test api call result";
            variableRule2.ConditionExpression = "apiResult.StatusCode == 200";

            apiCallRule.Rules.Add(variableRule2);

            rules.Add(apiCallRule.AsRuleEngineRule());
            workflow.Rules = rules;
            workflows.Add(workflow);

            return workflows;
        }

        private List<Workflow> CreatePostApiCallRuleWorkflow()
        {
            List<Workflow> workflows = new List<Workflow>();
            Workflow workflow = new Workflow();
            workflow.WorkflowName = "Test Workflow Rule 1";

            List<Rule> rules = new List<Rule>();

            var apiCallRule = new ApiCallRule();
            apiCallRule.Name = "Call test api";
            apiCallRule.Method = "POST";
            apiCallRule.Url = "http://httpbin.org/post";
            apiCallRule.ReturnVariableName = "apiResult";

            string jsonString = "{ \"test\": true }";
            apiCallRule.Content = jsonString;

            var variableRule2 = new VariableRule();
            variableRule2.Name = "Test api call result";
            variableRule2.Variables = new Dictionary<string, string>
            {
                { "apiBodyJson", "bool.Parse(apiResult.Body.json.test.ToString())" }
            };
            variableRule2.ConditionExpression = "apiResult.Body.json != null";

            var variableRule3 = new VariableRule();
            variableRule3.Name = "Test apiBodyJson";
            variableRule3.ConditionExpression = "apiBodyJson == \"true\"";

            variableRule2.Rules.Add(variableRule3);
            apiCallRule.Rules.Add(variableRule2);

            rules.Add(apiCallRule.AsRuleEngineRule());
            workflow.Rules = rules;
            workflows.Add(workflow);

            return workflows;
        }
    }
}
