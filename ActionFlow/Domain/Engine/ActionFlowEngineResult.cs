namespace ActionFlow.Domain.Engine
{
    public class ActionFlowEngineResult
    {
        public ActionFlowEngineResult()
        {
            OutputParameters = new Dictionary<string, object>();
        }

        public Dictionary<string, object> OutputParameters { get; set; }
    }
}
