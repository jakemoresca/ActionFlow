namespace ActionFlow.DB.ReadModels;

/// <summary>
/// Read model summarizing an execution, built by projecting the execution event stream. Backs the
/// API's <c>GET /executions/{id}</c> debugging endpoint. The stream id (= <c>ExecutionId</c>) is the
/// document id.
/// </summary>
public class WorkflowExecutionStatus
{
    public Guid Id { get; set; }
    public string WorkflowName { get; set; } = default!;
    public int? Version { get; set; }

    /// <summary>One of: Requested, Running, Completed, Failed, Compensated.</summary>
    public string Status { get; set; } = "Requested";

    public int CurrentStepIndex { get; set; } = -1;
    public List<string> CompletedSteps { get; set; } = new();
    public string? Error { get; set; }
    public Dictionary<string, string> OutputParameters { get; set; } = new();

    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
