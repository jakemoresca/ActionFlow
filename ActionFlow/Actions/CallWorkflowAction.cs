using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Actions
{
    public class CallWorkflowAction : ActionBase
    {
        public readonly static string WorkflowNameKey = "WorkflowName";
        public readonly static string ParametersKey = "Parameters";
        public readonly static string ResultVariableKey = "ResultVariable";

        public override string ActionType { get; } = "CallWorkFlow";

        public override async Task ExecuteAction()
        {
            var workflowName = ExecutionContext!.GetActionProperty<string>(WorkflowNameKey);
            var parameters = ExecutionContext!.GetActionProperty<Dictionary<string, string>>(ParametersKey);
            var resultVariable = ExecutionContext.GetActionProperty<string>(ResultVariableKey);

            var currentEngine = ExecutionContext.GetCurrentEngine();
            var executionContext = new ExecutionContext(currentEngine);

            if(parameters != null)
            {
                foreach (var parameter in parameters!)
                {
                    executionContext.AddOrUpdateParameter(parameter.Key, ExecutionContext.EvaluateExpression<object>(parameter.Value));
                }
            }

            var engineResult = await currentEngine.ExecuteWorkflowAsync(workflowName!, executionContext);

            if (resultVariable != null)
            {
                ExecutionContext.AddOrUpdateParameter(resultVariable, engineResult.OutputParameters);
            }
        }
    }
}
