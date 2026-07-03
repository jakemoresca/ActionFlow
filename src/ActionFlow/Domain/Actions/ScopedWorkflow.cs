using ActionFlow.Domain.Engine;

namespace ActionFlow.Domain.Actions
{
    public class ScopedWorkflow
    {
        public string? Expression { get; set; }
        public List<Step>? Steps { get; set; }
    }
}
