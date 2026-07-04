namespace ActionFlow.Runner.Sagas;

/// <summary>
/// Lifecycle states for <see cref="WorkflowExecutionSaga"/> (§7 of the microservices plan):
/// Requested → Running → Completed, or Running → Failing → Compensating → Compensated,
/// or Running → Failed (e.g. on timeout).
/// </summary>
public static class SagaStatus
{
    public const string Requested = nameof(Requested);
    public const string Running = nameof(Running);
    public const string Completed = nameof(Completed);
    public const string Failing = nameof(Failing);
    public const string Compensating = nameof(Compensating);
    public const string Compensated = nameof(Compensated);
    public const string Failed = nameof(Failed);

    public static bool IsTerminal(string status) =>
        status is Completed or Compensated or Failed;
}
