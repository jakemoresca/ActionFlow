using ActionFlow.Actions;
using ActionFlow.Engine.Factories;
using NSubstitute;

namespace ActionFlow.Tests.Engine
{
    [TestClass]
    public class StepActionFactoryTests
    {
        [TestMethod]
        public void When_adding_an_action_it_should_not_fail()
        {
            //Arrange
            var sut = new StepActionFactory();
            var actionName = "test";
            var fakeAction = Substitute.For<ActionBase>();

            //Act
            //Assert
            sut.AddOrUpdate(actionName, () => fakeAction);
        }

        [TestMethod]
        public void When_adding_an_existing_action_it_should_not_fail()
        {
            //Arrange
            var sut = new StepActionFactory();
            var actionName = "test";
            var fakeAction = Substitute.For<ActionBase>();

            sut.AddOrUpdate(actionName, () => fakeAction);

            //Act
            //Assert
            sut.AddOrUpdate(actionName, () => fakeAction);
        }

        [TestMethod]
        public void When_removing_an_existing_action_it_should_return_true()
        {
            //Arrange
            var sut = new StepActionFactory();
            var actionName = "test";
            var fakeAction = Substitute.For<ActionBase>();

            sut.AddOrUpdate(actionName, () => fakeAction);

            //Act
            var result = sut.Remove(actionName);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void When_removing_a_non_existing_action_it_should_return_false()
        {
            //Arrange
            var sut = new StepActionFactory();
            var actionName = "test";

            //Act
            var result = sut.Remove(actionName);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void When_getting_it_should_return_the_action()
        {
            //Arrange
            var sut = new StepActionFactory();
            var actionName = "test";
            var fakeAction = Substitute.For<ActionBase>();

            sut.AddOrUpdate(actionName, () => fakeAction);

            //Act
            var actionBase = sut.Get(actionName);

            //Assert
            Assert.AreEqual(fakeAction, actionBase);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException), "Action with name: test does not exist")]
        public void When_getting_non_existing_action_it_should_throw_exception()
        {
            //Arrange
            var sut = new StepActionFactory();
            var actionName = "test";

            //Act
            //Assert
            sut.Get(actionName);
        }

        [TestMethod]
        public void When_clearing_it_should_not_fail()
        {
            //Arrange
            var sut = new StepActionFactory();
            var actionName = "test";
            var fakeAction = Substitute.For<ActionBase>();

            sut.AddOrUpdate(actionName, () => fakeAction);

            //Act
            //Assert
            sut.Clear();
        }
    }
}
