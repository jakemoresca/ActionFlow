using ActionFlow.Domain.Engine;
using ActionFlow.Engine.Providers;
using ActionFlow.Engine.Providers.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace ActionFlow.Tests.Providers;

[TestClass]
public class WorkflowProviderBuiltinExtensionsTests
{
	[TestMethod]
	public void When_using_from_json_it_should_add_workflow()
	{
		//Arrange
		var workflows = new List<Workflow>
		{
			new("Test",
			[
				new("initialize", "Variable", new Dictionary<string, object>
				{
					{ "age", "1" },
					{ "canWalk", "true" },
				}),
				new("test variable value", "Variable", [], "age == 1 && canWalk == true")
			])
		};

		var workflowJson = JsonSerializer.SerializeToNode(workflows);
		var services = new ServiceCollection();
		services.AddJsonWorkflowProvider(workflowJson!.AsArray());

		//Act
		var provider = services.BuildServiceProvider();

		//Assert
		var workflowProvider = provider.GetRequiredService<IWorkflowProvider>();
		var result = workflowProvider.GetAllWorkflows();
		result.Should().BeEquivalentTo(workflows);
	}

	[TestMethod]
	public void When_using_from_json_file_it_should_add_workflow()
	{
		//Arrange
		var expected = new List<Workflow>
		{
			new("Test",
			[
				new("initialize", "Variable", new Dictionary<string, object>
				{
					{ "age", "1" },
					{ "canWalk", "true" },
				}),
				new("test variable value", "Variable", [], "age == 1 && canWalk == true")
			])
		};

		var services = new ServiceCollection();
		var path = Path.Combine(Environment.CurrentDirectory, @"Providers\workflows.json");
		services.AddJsonWorkflowProvider(path);

		//Act
		var provider = services.BuildServiceProvider();

		//Assert
		var workflowProvider = provider.GetRequiredService<IWorkflowProvider>();
		var result = workflowProvider.GetAllWorkflows();
		result.Should().BeEquivalentTo(expected);
	}
}
