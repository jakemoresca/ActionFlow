using RulesEngine.Models;

namespace ActionFlow.Engine
{
    public class WorkflowProvider : IWorkflowProvider
    {
        public List<Workflow> GetAllWorkflows()
        {
            List<Workflow> workflows = new List<Workflow>();

            //Test
            //Todo: Load from DB, Json, or other source
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
            //Test End

            return workflows;
        }
    }
}
