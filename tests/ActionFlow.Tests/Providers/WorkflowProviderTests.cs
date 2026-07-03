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
}
