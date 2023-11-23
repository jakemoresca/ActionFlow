using ActionFlow.Domain.Actions;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ActionFlow.Engine.Factories;

namespace ActionFlow.Actions
{
    public class ForLoopAction : ActionBase
    {
        public readonly static string InitializerVariableKey = "InitializerVariable";
        public readonly static string InitialValueKey = "InitialValue";
        public readonly static string LoopConditionKey = "Condition";
        public readonly static string IteratorKey = "Iterator";
        public readonly static string StepsKey = "Steps";

        public override string ActionType { get; } = "ForLoop";

        public override async Task ExecuteAction()
        {
            var initializerVariable = ExecutionContext!.GetActionProperty<string>(InitializerVariableKey);
            var initialValue = ExecutionContext!.GetActionProperty<string>(InitialValueKey);
            var loopCondition = ExecutionContext!.GetActionProperty<string>(LoopConditionKey);
            var iterator = ExecutionContext!.GetActionProperty<string>(IteratorKey);
            var steps = ExecutionContext!.GetActionProperty<List<Step>>(StepsKey);

            var currentEngine = ExecutionContext.GetCurrentEngine();

            if (initializerVariable != null && initialValue != null && loopCondition != null && iterator != null && steps != null)
            {
                var initialVariable = ExecutionContext.EvaluateExpression<int>(initialValue);
                ExecutionContext.AddOrUpdateParameter(initializerVariable, initialVariable);

                for (ExecutionContext.EvaluateExpression<int>(initializerVariable); ExecutionContext.EvaluateExpression<bool>(loopCondition); ExecutionContext.AddOrUpdateParameter(initializerVariable, ExecutionContext.EvaluateExpression<int>(iterator)))
                {
                    foreach (var step in steps)
                    {
                        ExecutionContext = await currentEngine.GetStepExecutionEvaluator().EvaluateAndRunStep(step, ExecutionContext, currentEngine.GetActionFactory());
                    }
                }
            }
        }
    }
}
