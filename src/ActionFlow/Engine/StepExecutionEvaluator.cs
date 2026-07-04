using System.Diagnostics;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine.Observers;

namespace ActionFlow.Engine
{
    public class StepExecutionEvaluator : IStepExecutionEvaluator
    {
        private readonly IStepExecutionObserver _observer;

        // A single required-observer constructor (rather than an optional parameter or a parameterless
        // overload): DI containers that build via code generation (e.g. Wolverine) skip optional
        // parameters and may prefer a parameterless constructor, silently falling back to the no-op
        // observer even when one is registered. UseActionFlowEngine always registers an observer
        // (NullStepExecutionObserver by default), so this stays satisfiable.
        public StepExecutionEvaluator(IStepExecutionObserver observer)
        {
            _observer = observer;
        }

        public async Task<ExecutionContext> EvaluateAndRunStep(Step step, ExecutionContext executionContext, IStepActionFactory stepActionFactory)
        {
            var shouldExecuteStep = step.ConditionExpression == null || executionContext.EvaluateExpression<bool>(step.ConditionExpression!);

            if (!shouldExecuteStep)
            {
                executionContext.ClearActionProperties();
                return executionContext;
            }

            await _observer.OnStepStartingAsync(step, executionContext);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var action = stepActionFactory.Get(step.ActionType);
                var updatedExecutionContext = BuildActionProperties(step, executionContext);
                action.SetExecutionContext(updatedExecutionContext);
                await action.ExecuteAction();

                stopwatch.Stop();
                await _observer.OnStepCompletedAsync(step, executionContext, stopwatch.Elapsed);
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                await _observer.OnStepFailedAsync(step, executionContext, exception);
                throw;
            }
            finally
            {
                executionContext.ClearActionProperties();
            }

            return executionContext;
        }

        private static ExecutionContext BuildActionProperties(Step step, ExecutionContext executionContext)
        {
            foreach (var property in step.Properties!)
            {
                executionContext.AddOrUpdateActionProperty(property.Key, property.Value);
            }

            return executionContext;
        }
    }
}
