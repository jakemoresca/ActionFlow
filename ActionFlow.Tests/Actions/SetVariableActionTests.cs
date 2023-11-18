﻿using ActionFlow.Actions;

namespace ActionFlow.Tests.Actions
{
    [TestClass]
    public class SetVariableActionTests
    {
        [TestMethod]
        public void When_executing_it_should_set_new_variable()
        {
            //Arrange
            var sut = new SetVariableAction();
            var executionContext = new ActionFlow.Engine.ExecutionContext();
            var variables = new Dictionary<string, string>
            {
                { "age", "18" },
                { "canWalk", "true" }
            };
            executionContext.AddOrUpdateActionProperty(SetVariableAction.VariablesKey, variables);
            sut.SetExecutionContext(executionContext);

            //Act
            sut.ExecuteAction();

            //Assert
            Assert.AreEqual(18, executionContext.EvaluateExpression<int>("age"));
            Assert.AreEqual(true, executionContext.EvaluateExpression<bool>("canWalk"));
        }
    }
}
