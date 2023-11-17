namespace ActionFlow.Domain.Engine
{
    public class Workflow
    {
        public Workflow(string workFlowName, List<Step> steps, List<Parameter>? inputParameters = null, List<Parameter>? outputParameters = null)
        {
            WorkflowName = workFlowName;
            Steps = steps;
            InputParameters = inputParameters;
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

        List<Parameter>? InputParameters { get; }
        List<Parameter>? OutputParameters { get; }
    }
}
