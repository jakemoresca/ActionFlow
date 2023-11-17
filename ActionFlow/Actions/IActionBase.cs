namespace ActionFlow.Actions
{
    public interface IActionBase
    {
        string ActionType { get; }

        void ExecuteAction();
        void SetExecutionContext(ExecutionContext executionContext);
    }
}