using ActionFlow.Actions;

namespace ActionFlow.Tests.Actions
{
    [TestClass]
    public class SetVariableActionTests : ActionBaseTests
    {
        [TestMethod]
        public async Task When_executing_it_should_set_new_variable()
        {
            //Arrange
            var sut = new SetVariableAction();
            var executionContext = ExecutionContext;
            var variables = new Dictionary<string, string>
            {
                { "age", "18" },
                { "canWalk", "true" }
            };
            executionContext.AddOrUpdateActionProperty(SetVariableAction.VariablesKey, variables);
            sut.SetExecutionContext(executionContext);

            //Act
            await sut.ExecuteAction();

            //Assert
            Assert.AreEqual(18, executionContext.EvaluateExpression<int>("age"));
            Assert.AreEqual(true, executionContext.EvaluateExpression<bool>("canWalk"));
        }
    }
}
