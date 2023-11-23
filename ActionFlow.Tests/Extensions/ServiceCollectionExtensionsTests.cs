using ActionFlow.Actions;
using ActionFlow.Engine;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine.Providers;
using ActionFlow.Extensions;
using ActionFlow.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
