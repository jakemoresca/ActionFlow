namespace ActionFlow.Domain.Engine
{
    public class ActionFlowEngineResult
    {
        public ActionFlowEngineResult()
        {
            OutputParameters = [];
        }

        public Dictionary<string, object> OutputParameters { get; set; }
    }
}
