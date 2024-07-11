using ActionFlow.Domain.Engine;

namespace ActionFlow.Engine.Providers
{
	public class BlankWorkflowProvider() : WorkflowProvider(new List<Workflow>())
	{

	}

	public class WorkflowProvider(List<Workflow> workflows) : IWorkflowProvider
	{
		public List<Workflow> GetAllWorkflows()
		{
			//Test
			//Todo: Load from DB, Json, or other source
			//var steps = new List<Step>
			//{
			//	new("initialize", "Variable", new Dictionary<string, object>
			//	{
			//		{ "age", "1" },
			//		{ "canWalk", "true" },
			//	}),
			//	new("test variable value", "Variable", [], "age == 1 && canWalk == true")
			//};

			//Workflow workflow = new Workflow("Test Workflow Rule 1", steps);
			//workflows.Add(workflow);
			////Test End

			return workflows;
		}
	}
}
