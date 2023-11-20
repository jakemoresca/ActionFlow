using ActionFlow.Domain.Actions;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ActionFlow.Engine.Factories;

namespace ActionFlow.Actions
{
    public class ControlFlowAction : ActionBase
    {
        public readonly static string ConditionsKey = "Conditions";

        private readonly IStepExecutionEvaluator _stepExecutionEvaluator;
        private readonly IStepActionFactory _stepActionFactory;

        public ControlFlowAction(IStepExecutionEvaluator stepExecutionEvaluator, IStepActionFactory stepActionFactory)
        {
            _stepExecutionEvaluator = stepExecutionEvaluator;
            _stepActionFactory = stepActionFactory;
        }

        public override async Task ExecuteAction()
        {
            var scopedWorkflows = ExecutionContext!.GetActionProperty<List<ScopedWorkflow>>(ConditionsKey);

            if (scopedWorkflows != null)
            {
                foreach (var scopedWorkflow in scopedWorkflows)
                {
                    if (!ExecutionContext.EvaluateExpression<bool>(scopedWorkflow.Expression))
                        continue;

                    var workflow = CreateScopedWorkflow(scopedWorkflow.Steps);

                    foreach (var step in workflow.Steps)
                    {
                        ExecutionContext = await _stepExecutionEvaluator.EvaluateAndRunStep(step, ExecutionContext, _stepActionFactory);
                    }
                }
            }
        }

        private Workflow CreateScopedWorkflow(List<Step> steps)
        {
            var name = $"[ScopedWorkflow]_{Guid.NewGuid()}";
            return new Workflow(name, steps);
        }
    }
}
