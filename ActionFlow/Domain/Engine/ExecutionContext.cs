namespace ActionFlow.Domain.Engine
{
    public class ExecutionContext
    {
        public ExecutionContext()
        {
            Parameters = new List<Parameter>();
        }

        public List<Parameter> Parameters { get; set; }
    }
}
