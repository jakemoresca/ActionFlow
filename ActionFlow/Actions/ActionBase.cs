namespace ActionFlow.Actions
{
    public abstract class ActionBase : IActionBase
    {
        public virtual string ActionType { get; } = string.Empty;
        private ExecutionContext _executionContext;

        public void SetExecutionContext(ExecutionContext executionContext)
        {
            throw new NotImplementedException();
        }

        public virtual void ExecuteAction()
        {
        }
    }
}
