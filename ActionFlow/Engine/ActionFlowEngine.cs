using ActionFlow.Domain.Engine;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine.Providers;
using ExecutionContext = ActionFlow.Domain.Engine.ExecutionContext;

namespace ActionFlow.Engine
{
    public class ActionFlowEngine : IActionFlowEngine
    {
        private readonly IWorkflowProvider _workflowProvider;
        private readonly IStepActionFactory _stepActionFactory;
        private readonly IStepExecutionEvaluator _stepExecutionEvaluator;
        private Dictionary<string, Workflow> _workflows = new Dictionary<string, Workflow>();

        public ActionFlowEngine(IWorkflowProvider workflowProvider, IStepActionFactory stepActionFactory, IStepExecutionEvaluator stepExecutionEvaluator)
        {
            _workflowProvider = workflowProvider;
            _stepActionFactory = stepActionFactory;
            _stepExecutionEvaluator = stepExecutionEvaluator;
        }

        /// <summary>
        /// This will execute all the rules of the specified workflow
        /// </summary>
        /// <param name="workflowName">The name of the workflow with rules to execute against the inputs</param>
        /// <param name="inputs">A variable number of inputs</param>
        /// <returns>List of rule results</returns>
        public async ValueTask<ActionFlowEngineResult> ExecuteWorkflowAsync(string workflowName, params Parameter[] inputs)
        {
            var executionContext = BuildExecutionContext(inputs);
            return await ExecuteWorkflowAsync(workflowName, executionContext);
        }

        /// <summary>
        /// This will execute all the rules of the specified workflow
        /// </summary>
        /// <param name="workflowName">The name of the workflow with rules to execute against the inputs</param>
        /// <param name="executionContext">Execution context to use</param>
        /// <returns>List of rule results</returns>
        public async ValueTask<ActionFlowEngineResult> ExecuteWorkflowAsync(string workflowName, ExecutionContext executionContext)
        {
            var workflow = GetWorkflow(workflowName);
            var updatedExecutionContext = executionContext;

            foreach (var step in workflow.Steps)
            {
                updatedExecutionContext = _stepExecutionEvaluator.EvaluateAndRunStep(step, updatedExecutionContext, _stepActionFactory);
            }

            return await Task.FromResult(new ActionFlowEngineResult());
        }

        private Workflow GetWorkflow(string name)
        {
            if (_workflows.Count == 0)
            {
                _workflows = _workflowProvider.GetAllWorkflows().ToDictionary(x => x.WorkflowName, x => x);
            }

            return _workflows[name];
        }

        private ExecutionContext BuildExecutionContext(params Parameter[] inputs)
        {
            var executionContext = new ExecutionContext();
            executionContext.Parameters.AddRange(inputs);
            return executionContext;
        }
    }
}
