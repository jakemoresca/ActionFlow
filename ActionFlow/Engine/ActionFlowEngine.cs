using ActionFlow.Domain.Engine;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine.Providers;

namespace ActionFlow.Engine
{
	public class ActionFlowEngine(IWorkflowProvider workflowProvider, IStepActionFactory stepActionFactory, IStepExecutionEvaluator stepExecutionEvaluator, IHelperProvider helperProvider) : IActionFlowEngine
	{
		private Dictionary<string, Workflow> _workflows = [];

		public IStepActionFactory GetActionFactory() => stepActionFactory;
		public IStepExecutionEvaluator GetStepExecutionEvaluator() => stepExecutionEvaluator;

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
				updatedExecutionContext = await stepExecutionEvaluator.EvaluateAndRunStep(step, updatedExecutionContext, stepActionFactory);
			}

			var result = new ActionFlowEngineResult();

			if (workflow.OutputParameters != null)
			{
				result.OutputParameters = GetOutputParameters(workflow.OutputParameters, updatedExecutionContext);
			}

			return await Task.FromResult(result);
		}

		private Workflow GetWorkflow(string name)
		{
			if (_workflows.Count == 0)
			{
				_workflows = workflowProvider.GetAllWorkflows().ToDictionary(x => x.WorkflowName, x => x);
			}

			return _workflows[name];
		}

		private ExecutionContext BuildExecutionContext(params Parameter[] inputs)
		{
			var executionContext = new ExecutionContext(this);

			foreach (var input in inputs)
			{
				executionContext.AddOrUpdateParameter(input);
			}

			return executionContext;
		}

		private static Dictionary<string, object> GetOutputParameters(List<Parameter> parameters, ExecutionContext executionContext)
		{
			var output = new Dictionary<string, object>();

			foreach (var parameter in parameters)
			{
				output.Add(parameter.Name!, executionContext.EvaluateExpression<object>(parameter.Expression!));
			}

			return output;
		}
	}
}
