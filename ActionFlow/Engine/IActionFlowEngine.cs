using ActionFlow.Domain.Engine;
using ActionFlow.Engine.Factories;
using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Engine
{
    public interface IActionFlowEngine
    {
        ValueTask<ActionFlowEngineResult> ExecuteWorkflowAsync(string workflowName, params Parameter[] inputs);
        ValueTask<ActionFlowEngineResult> ExecuteWorkflowAsync(string workflowName, ExecutionContext executionContext);
        IStepActionFactory GetActionFactory();
        IStepExecutionEvaluator GetStepExecutionEvaluator();
    }
}