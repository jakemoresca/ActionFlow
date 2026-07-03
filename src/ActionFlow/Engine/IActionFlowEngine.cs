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

        /// <summary>
        /// Evicts a workflow (or the whole cache when <paramref name="name"/> is null) from the
        /// engine's in-memory cache so it is reloaded from the provider on next execution.
        /// </summary>
        void InvalidateWorkflow(string? name = null);
    }
}