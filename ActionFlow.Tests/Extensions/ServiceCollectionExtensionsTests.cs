using ActionFlow.Actions;
using ActionFlow.Engine;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine.Providers;
using ActionFlow.Extensions;
using ActionFlow.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace ActionFlow.Tests.Extensions
{
	[TestClass]
	public class ServiceCollectionExtensionsTests
	{
		[TestMethod]
		public void When_using_action_flow_engine_it_should_register_all_required_services_and_actions()
		{
			//Arrange
			var services = new ServiceCollection();
			services.UseActionFlowEngine();

			//Act
			var provider = services.BuildServiceProvider();

			//Assert
			Assert.IsNotNull(provider.GetRequiredService<IActionFlowEngine>());
			Assert.IsNotNull(provider.GetRequiredService<IWorkflowProvider>());
			Assert.IsNotNull(provider.GetRequiredService<IStepExecutionEvaluator>());
			Assert.IsNotNull(provider.GetRequiredService<IStepActionFactory>());
			Assert.IsNotNull(provider.GetRequiredService<IApiClient>());
			Assert.IsNotNull(provider.GetRequiredService<IEnumerable<IActionBase>>());

			var stepActionFactory = provider.GetRequiredService<IStepActionFactory>();
			Assert.AreEqual("CallWorkFlow", stepActionFactory.Get("CallWorkFlow").ActionType);
			Assert.AreEqual("ControlFlow", stepActionFactory.Get("ControlFlow").ActionType);
			Assert.AreEqual("ForLoop", stepActionFactory.Get("ForLoop").ActionType);
			Assert.AreEqual("SendHttpCall", stepActionFactory.Get("SendHttpCall").ActionType);
			Assert.AreEqual("SetVariable", stepActionFactory.Get("SetVariable").ActionType);
		}
	}
}
