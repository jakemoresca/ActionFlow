using Wolverine;

namespace ActionFlow.Runner.Sagas;

/// <summary>
/// Internal (in-process) command the saga cascades to actually run the engine. Kept out of
/// <c>ActionFlow.Contracts</c> because it never crosses the broker — it is routed to the local
/// <c>RunWorkflowExecutionHandler</c> handler.
/// </summary>
public record RunWorkflowExecution(
    Guid ExecutionId,
    string WorkflowName,
    int? Version,
    Dictionary<string, string> Inputs);

/// <summary>
/// Saga timeout (§7). Inheriting <see cref="TimeoutMessage"/> tells Wolverine this is a saga deadline:
/// it is scheduled with a delay and automatically discarded if the saga has already completed. If it
/// does fire, the saga treats the execution as failed.
/// </summary>
public record WorkflowExecutionTimeout(Guid ExecutionId) : TimeoutMessage(DefaultDeadline)
{
    /// <summary>How long the saga waits for a terminal event before declaring a timeout.</summary>
    public static readonly TimeSpan DefaultDeadline = TimeSpan.FromMinutes(5);
}
