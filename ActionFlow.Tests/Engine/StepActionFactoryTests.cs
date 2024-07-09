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
            var actions = CreateActionDictionary();
            var sut = new StepActionFactory(actions);
            var actionName = "test";
            var fakeAction = Substitute.For<ActionBase>();

            //Act
            var result = sut.AddOrUpdate(actionName, () => fakeAction);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void When_adding_an_existing_action_it_should_not_fail()
        {
            //Arrange
            var actions = CreateActionDictionary();
            var sut = new StepActionFactory(actions);
            var actionName = "test";
            var fakeAction = Substitute.For<ActionBase>();

            sut.AddOrUpdate(actionName, () => fakeAction);

            //Act
            var result = sut.AddOrUpdate(actionName, () => fakeAction);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void When_removing_an_existing_action_it_should_return_true()
        {
            //Arrange
            var actions = CreateActionDictionary();
            var sut = new StepActionFactory(actions);
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
            var actions = CreateActionDictionary();
            var sut = new StepActionFactory(actions);
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
            var actions = CreateActionDictionary();
            var sut = new StepActionFactory(actions);
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
            var actions = CreateActionDictionary();
            var sut = new StepActionFactory(actions);
            var actionName = "test";

            //Act
            var result = sut.Get(actionName);

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void When_clearing_it_should_not_fail()
        {
            //Arrange
            var actions = CreateActionDictionary();
            var sut = new StepActionFactory(actions);
            var actionName = "test";
            var fakeAction = Substitute.For<ActionBase>();

            sut.AddOrUpdate(actionName, () => fakeAction);

            //Act
            var result = sut.Clear();

            //Assert
            Assert.IsTrue(result);
        }

        private static IEnumerable<ActionBase> CreateActionDictionary()
        {
            var fakeAction = Substitute.For<ActionBase>();
            
            return new List<ActionBase> { { fakeAction } };
        }
    }
}
