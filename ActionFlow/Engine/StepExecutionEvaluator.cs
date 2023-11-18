using ActionFlow.Domain.Engine;
using ActionFlow.Engine.Factories;

namespace ActionFlow.Engine
{
    public class StepExecutionEvaluator : IStepExecutionEvaluator
    {
        public ExecutionContext EvaluateAndRunStep(Step step, ExecutionContext executionContext, IStepActionFactory stepActionFactory)
        {
            var shouldExecuteStep = executionContext.EvaluateExpression<bool>(step.ConditionExpression!);

            if (shouldExecuteStep)
            {
                var action = stepActionFactory.Get(step.ActionType);
                var updatedExecutionContext = BuildActionProperties(step, executionContext);
                action.SetExecutionContext(updatedExecutionContext);
                action.ExecuteAction();
            }

            executionContext.ClearActionProperties();

            return executionContext;
        }

        private ExecutionContext BuildActionProperties(Step step, ExecutionContext executionContext) 
        {
            foreach (var property in step.Properties!)
            {
                executionContext.AddOrUpdateActionProperty(property.Key, property.Value);
            }

            return executionContext;
        }
    }
}
