using ActionFlow.Actions;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Tests.Actions
{
    public abstract class ActionBaseTests
    {
        public IActionFlowEngine ActionFlowEngine { get; }
        public ExecutionContext ExecutionContext { get; }

        public ActionBaseTests()
        {
            var actionFlowEngine = Substitute.For<IActionFlowEngine>();
            var stepActionFactory = Substitute.For<IStepActionFactory>();
            var stepExecutionEvaluator = Substitute.For<IStepExecutionEvaluator>();

            stepActionFactory.Get("Variable").Returns(new SetVariableAction());

            actionFlowEngine.GetActionFactory().Returns(stepActionFactory);
            actionFlowEngine.GetStepExecutionEvaluator().Returns(stepExecutionEvaluator);

            ActionFlowEngine = actionFlowEngine;
            ExecutionContext = new ExecutionContext(actionFlowEngine);
        }
    }
}
