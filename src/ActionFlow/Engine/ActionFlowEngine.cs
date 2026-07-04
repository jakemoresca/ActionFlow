using ActionFlow.Domain.Engine;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine.Providers;

namespace ActionFlow.Engine
{
	public class ActionFlowEngine(IWorkflowProvider workflowProvider, IStepActionFactory stepActionFactory, IStepExecutionEvaluator stepExecutionEvaluator) : IActionFlowEngine
	{
		private readonly Dictionary<string, Workflow> _workflows = [];

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
			var workflow = await GetWorkflowAsync(workflowName);
			var updatedExecutionContext = executionContext;

			for (var index = 0; index < workflow.Steps.Count; index++)
			{
				updatedExecutionContext.CurrentStepIndex = index;
				// EvaluateAndRunStep returns the same context instance; fall back to it defensively.
				updatedExecutionContext = await stepExecutionEvaluator.EvaluateAndRunStep(workflow.Steps[index], updatedExecutionContext, stepActionFactory)
					?? updatedExecutionContext;
			}

			var result = new ActionFlowEngineResult();

			if (workflow.OutputParameters != null)
			{
				result.OutputParameters = GetOutputParameters(workflow.OutputParameters, updatedExecutionContext);
			}

			return await Task.FromResult(result);
		}

		/// <summary>
		/// Resolves a workflow by name, lazily loading it from the provider and caching it.
		/// Unknown names surface as <see cref="KeyNotFoundException"/> to preserve the previous
		/// dictionary-indexing behavior.
		/// </summary>
		private async ValueTask<Workflow> GetWorkflowAsync(string name, CancellationToken cancellationToken = default)
		{
			if (_workflows.TryGetValue(name, out var cached))
			{
				return cached;
			}

			var workflow = await workflowProvider.GetWorkflowAsync(name, cancellationToken)
				?? throw new KeyNotFoundException($"Workflow '{name}' was not found.");

			_workflows[name] = workflow;
			return workflow;
		}

		/// <summary>
		/// Evicts a workflow from the in-memory cache so the next execution reloads it from the
		/// provider. Used for event-driven cache invalidation (e.g. on <c>WorkflowPublished</c>).
		/// Pass <c>null</c> to clear the whole cache.
		/// </summary>
		public void InvalidateWorkflow(string? name = null)
		{
			if (name is null)
			{
				_workflows.Clear();
			}
			else
			{
				_workflows.Remove(name);
			}
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
