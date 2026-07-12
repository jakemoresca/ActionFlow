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

		/// <summary>
		/// When true, the engine emits every persistent parameter from the final
		/// execution context as output (in addition to any declared
		/// <see cref="OutputParameters"/>).
		/// </summary>
		public bool OutputAllParameters { get; set; }
	}
}
