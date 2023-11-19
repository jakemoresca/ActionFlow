namespace ActionFlow.Domain.Engine
{
    public class Workflow
    {
        public Workflow(string workFlowName, List<Step> steps, List<Parameter>? outputParameters = null)
        {
            WorkflowName = workFlowName;
            Steps = steps;
            OutputParameters = outputParameters;
        }

        /// <summary>
        /// Gets the workflow name.
        /// </summary>
        public string WorkflowName { get; }

        /// <summary>
        /// Ordered list of steps to execute
        /// </summary>
        public List<Step> Steps { get; }
        public List<Parameter>? OutputParameters { get; }
    }
}
