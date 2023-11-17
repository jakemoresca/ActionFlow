using ActionFlow.Domain.Engine;
using ActionFlow.Engine.Factories;
using DynamicExpresso;
using ExecutionContext = ActionFlow.Domain.Engine.ExecutionContext;

namespace ActionFlow.Engine
{
    public class StepExecutionEvaluator : IStepExecutionEvaluator
    {
        public ExecutionContext EvaluateAndRunStep(Step step, ExecutionContext executionContext, IStepActionFactory stepActionFactory)
        {
            var interpreter = BuildInterpreterVariables(new Interpreter(), executionContext);

            var shouldExecuteStep = interpreter.Eval<bool>(step.ConditionExpression);

            if (shouldExecuteStep)
            {
                var action = stepActionFactory.Get(step.ActionType);
                //run step action
                //build new execution context
            }

            return executionContext;
        }

        private Interpreter BuildInterpreterVariables(Interpreter interpreter, ExecutionContext executionContext)
        {
            foreach (var parameter in executionContext.Parameters!)
            {
                interpreter.SetVariable(parameter.Name, interpreter.Eval(parameter.Expression));
            }

            return interpreter;
        }
    }
}
