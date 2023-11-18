using ActionFlow.Domain.Engine;
using ActionFlow.Engine.Factories;

namespace ActionFlow.Engine
{
    public interface IStepExecutionEvaluator
    {
        ExecutionContext EvaluateAndRunStep(Step step, ExecutionContext executionContext, IStepActionFactory stepActionFactory);
    }
}