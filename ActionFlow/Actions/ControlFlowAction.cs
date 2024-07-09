using ActionFlow.Domain.Actions;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine;

namespace ActionFlow.Actions
{
	public class ControlFlowAction : ActionBase
	{
		public readonly static string ConditionsKey = "Conditions";

		public override string ActionType { get; } = "ControlFlow";

		public override async Task ExecuteAction()
		{
			var scopedWorkflows = ExecutionContext!.GetActionProperty<List<ScopedWorkflow>>(ConditionsKey);
			var currentEngine = ExecutionContext.GetCurrentEngine();

			if (scopedWorkflows != null)
			{
				foreach (var scopedWorkflow in scopedWorkflows)
				{
					if (!ExecutionContext.EvaluateExpression<bool>(scopedWorkflow.Expression!))
						continue;

					var workflow = CreateScopedWorkflow(scopedWorkflow.Steps!);

					foreach (var step in workflow.Steps)
					{
						ExecutionContext = await currentEngine.GetStepExecutionEvaluator().EvaluateAndRunStep(step, ExecutionContext, currentEngine.GetActionFactory());
					}
				}
			}
		}

		private static Workflow CreateScopedWorkflow(List<Step> steps)
		{
			var name = $"[ScopedWorkflow]_{Guid.NewGuid()}";
			return new Workflow(name, steps);
		}
	}
}
