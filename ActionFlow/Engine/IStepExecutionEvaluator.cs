using ActionFlow.Domain.Engine;
using ActionFlow.Engine.Factories;
using ExecutionContext = ActionFlow.Domain.Engine.ExecutionContext;

namespace ActionFlow.Engine
{
    public interface IStepExecutionEvaluator
    {
        ExecutionContext EvaluateAndRunStep(Step step, ExecutionContext executionContext, IStepActionFactory stepActionFactory);
    }
}