namespace ActionFlow.Contracts;

/// <summary>
/// Kafka topic names shared by the API (producer) and the Runner (consumer/producer).
/// Per-execution ordering is achieved by using <c>ExecutionId</c> as the partition key.
/// </summary>
public static class Topics
{
    /// <summary>Point-to-point <see cref="ExecuteWorkflow"/> / <see cref="CompensateWorkflow"/> commands.</summary>
    public const string ExecuteCommands = "actionflow.commands.execute";

    /// <summary>Execution audit/integration events (Started, StepCompleted, StepFailed, Completed, Failed, ...).</summary>
    public const string ExecutionEvents = "actionflow.events.execution";

    /// <summary>Workflow lifecycle events (<see cref="WorkflowPublished"/>).</summary>
    public const string WorkflowEvents = "actionflow.events.workflow";
}
