using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Actions
{
    public interface IActionBase
    {
        string ActionType { get; }

        Task ExecuteAction();
        void SetExecutionContext(ExecutionContext executionContext);
    }
}