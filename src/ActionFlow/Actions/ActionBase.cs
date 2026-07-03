using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Actions
{
    public abstract class ActionBase : IActionBase
    {
        public virtual string ActionType { get; } = string.Empty;
        internal ExecutionContext? ExecutionContext;

        public void SetExecutionContext(ExecutionContext executionContext)
        {
            ExecutionContext = executionContext;
        }

        public virtual Task ExecuteAction()
        {
            return Task.CompletedTask;
        }
    }
}
