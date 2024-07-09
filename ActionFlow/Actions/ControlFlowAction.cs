using ActionFlow.Domain.Actions;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ActionFlow.Engine.Factories;

namespace ActionFlow.Actions
{
    public class ControlFlowAction(IStepExecutionEvaluator stepExecutionEvaluator, IStepActionFactory stepActionFactory) : ActionBase
    {
        public readonly static string ConditionsKey = "Conditions";

		public override async Task ExecuteAction()
        {
            var scopedWorkflows = ExecutionContext!.GetActionProperty<List<ScopedWorkflow>>(ConditionsKey);

            if (scopedWorkflows != null)
            {
                foreach (var scopedWorkflow in scopedWorkflows)
                {
                    if (!ExecutionContext.EvaluateExpression<bool>(scopedWorkflow.Expression!))
                        continue;

                    var workflow = CreateScopedWorkflow(scopedWorkflow.Steps!);

                    foreach (var step in workflow.Steps)
                    {
                        ExecutionContext = await stepExecutionEvaluator.EvaluateAndRunStep(step, ExecutionContext, stepActionFactory);
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
