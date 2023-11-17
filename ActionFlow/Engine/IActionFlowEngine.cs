using ActionFlow.Domain.Engine;
using ExecutionContext = ActionFlow.Domain.Engine.ExecutionContext;

namespace ActionFlow.Engine
{
    public interface IActionFlowEngine
    {
        ValueTask<ActionFlowEngineResult> ExecuteWorkflowAsync(string workflowName, params Parameter[] inputs);
        ValueTask<ActionFlowEngineResult> ExecuteWorkflowAsync(string workflowName, ExecutionContext executionContext);
    }
}