namespace ActionFlow.Contracts;

// Point-to-point commands. Every message carries ExecutionId, which is also used as the Kafka
// partition key so all messages for one execution keep per-execution ordering.

/// <summary>
/// Request to execute a workflow. Published by ActionFlow.API (via the transactional outbox) and
/// consumed by the ActionFlow.Runner saga.
/// </summary>
public record ExecuteWorkflow
{
    public required Guid ExecutionId { get; init; }
    public required string WorkflowName { get; init; }

    /// <summary>Optional pinned version; when null the latest published version is used.</summary>
    public int? Version { get; init; }

    /// <summary>Input parameters as name → expression, mirroring the engine's string-expression model.</summary>
    public Dictionary<string, string> Inputs { get; init; } = new();

    public Guid CorrelationId { get; init; }
}

/// <summary>
/// Instructs the saga to compensate an execution's completed steps in reverse order up to (and
/// including) <see cref="ThroughStepIndex"/>.
/// </summary>
public record CompensateWorkflow
{
    public required Guid ExecutionId { get; init; }
    public required int ThroughStepIndex { get; init; }
}
