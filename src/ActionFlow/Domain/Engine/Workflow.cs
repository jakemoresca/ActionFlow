namespace ActionFlow.Domain.Engine
{
    public class Workflow(string workFlowName, List<Step> steps, List<Parameter>? outputParameters = null)
	{

		/// <summary>
		/// Gets the workflow name.
		/// </summary>
		public string WorkflowName { get; } = workFlowName;

		/// <summary>
		/// Ordered list of steps to execute
		/// </summary>
		public List<Step> Steps { get; } = steps;
		public List<Parameter>? OutputParameters { get; } = outputParameters;
	}
}
