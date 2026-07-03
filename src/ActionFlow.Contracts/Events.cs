namespace ActionFlow.Contracts;

/// <summary>
/// Integration + audit events (pub/sub). These are the raw material for the execution event
/// stream and the API's debugging timeline. Contracts are versioned additively; breaking shape
/// changes get a new <c>V2</c> type plus an upcaster.
/// </summary>

/// <summary>Emitted by the API when a workflow definition is created/updated so Runners refresh their caches.</summary>
public record WorkflowPublished
{
    public required string Name { get; init; }
    public required int Version { get; init; }
    public DateTimeOffset PublishedAt { get; init; }
}

/// <summary>Appended to a new execution stream when the API accepts an execution request.</summary>
public record WorkflowExecutionRequested
{
    public required Guid ExecutionId { get; init; }
    public required string WorkflowName { get; init; }
    public int? Version { get; init; }
    public DateTimeOffset RequestedAt { get; init; }
}

/// <summary>Emitted by the saga once it begins running the workflow.</summary>
public record WorkflowExecutionStarted
{
    public required Guid ExecutionId { get; init; }
    public DateTimeOffset StartedAt { get; init; }
}

/// <summary>Emitted per successfully executed step (drives the debug timeline + fine-grained compensation).</summary>
public record StepCompleted
{
    public required Guid ExecutionId { get; init; }
    public required string StepName { get; init; }
    public required int Index { get; init; }
    public long DurationMs { get; init; }
}

/// <summary>Emitted when a step throws.</summary>
public record StepFailed
{
    public required Guid ExecutionId { get; init; }
    public required string StepName { get; init; }
    public required int Index { get; init; }
    public required string Error { get; init; }
}

/// <summary>Terminal success event carrying the workflow's evaluated output parameters.</summary>
public record WorkflowExecutionCompleted
{
    public required Guid ExecutionId { get; init; }
    public Dictionary<string, string> OutputParameters { get; init; } = new();
}

/// <summary>Terminal failure event; <see cref="LastCompletedStepIndex"/> bounds any compensation.</summary>
public record WorkflowExecutionFailed
{
    public required Guid ExecutionId { get; init; }
    public required string Error { get; init; }
    public int LastCompletedStepIndex { get; init; } = -1;
}

/// <summary>Emitted once compensation of a failed execution finishes.</summary>
public record WorkflowExecutionCompensated
{
    public required Guid ExecutionId { get; init; }
}
