using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Actions
{
    public class CallWorkflowAction : ActionBase
    {
        public static string WorkflowNameKey = "WorkflowName";
        public static string ParametersKey = "Parameters";
        public static string ResultVariableKey = "ResultVariable";

        private readonly IActionFlowEngine _actionFlowEngine;

        public CallWorkflowAction(IActionFlowEngine actionFlowEngine)
        {
            _actionFlowEngine = actionFlowEngine;
        }

        public override async Task ExecuteAction()
        {
            var workflowName = ExecutionContext!.GetActionProperty<string>(WorkflowNameKey);
            var parameters = ExecutionContext!.GetActionProperty<Dictionary<string, string>>(ParametersKey);
            var resultVariable = ExecutionContext.GetActionProperty<string>(ResultVariableKey);

            var executionContext = new ExecutionContext();

            if(parameters != null)
            {
                foreach (var parameter in parameters!)
                {
                    executionContext.AddOrUpdateParameter(parameter.Key, ExecutionContext.EvaluateExpression<object>(parameter.Value));
                }
            }

            var engineResult = await _actionFlowEngine.ExecuteWorkflowAsync(workflowName!, executionContext);

            if (resultVariable != null)
            {
                ExecutionContext.AddOrUpdateParameter(resultVariable, engineResult.OutputParameters);
            }
        }
    }
}
