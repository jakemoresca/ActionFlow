using ActionFlow.Domain.Engine;
using ActionFlow.Engine.Providers;

namespace ActionFlow.Tests.Providers;

[TestClass]
public class WorkflowProviderTests
{
	[TestMethod]
	public void When_getting_all_workflows_it_should_return_a_list()
	{
		//Arrange
		var testWorkflows = new List<Workflow>
		{
			new("test", [])
		};
		var sut = new WorkflowProvider(testWorkflows);

		//Act
		var result = sut.GetAllWorkflows();

		//Assert
		Assert.IsInstanceOfType(result, typeof(List<Workflow>));
		Assert.IsTrue(result.Contains(testWorkflows[0]));
	}

	[TestMethod]
	public async Task GetWorkflowAsync_default_implementation_resolves_by_name()
	{
		//Arrange
		var testWorkflows = new List<Workflow>
		{
			new("first", []),
			new("second", [])
		};
		IWorkflowProvider sut = new WorkflowProvider(testWorkflows);

		//Act
		var result = await sut.GetWorkflowAsync("second");

		//Assert
		Assert.AreSame(testWorkflows[1], result);
	}

	[TestMethod]
	public async Task GetWorkflowAsync_default_implementation_returns_null_for_unknown_name()
	{
		//Arrange
		IWorkflowProvider sut = new WorkflowProvider([new("first", [])]);

		//Act
		var result = await sut.GetWorkflowAsync("missing");

		//Assert
		Assert.IsNull(result);
	}
}
