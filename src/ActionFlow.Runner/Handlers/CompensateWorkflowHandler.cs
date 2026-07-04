using ActionFlow.Contracts;
using Microsoft.Extensions.Logging;

namespace ActionFlow.Runner.Handlers;

/// <summary>
/// Performs coarse-grained compensation for a failed execution and reports completion back to the saga
/// via <see cref="WorkflowExecutionCompensated"/>.
/// </summary>
/// <remarks>
/// Baseline (Phase 3): the built-in actions declare no compensation, so there is nothing to undo — this
/// logs and reports compensated. Phase 5 introduces <c>ICompensableAction</c> and replays each
/// compensatable step in reverse through <c>ThroughStepIndex</c>.
/// </remarks>
public class CompensateWorkflowHandler
{
    public WorkflowExecutionCompensated Handle(CompensateWorkflow command, ILogger<CompensateWorkflowHandler> logger)
    {
        logger.LogWarning(
            "Compensating execution {ExecutionId} through step index {Index}: no compensatable actions registered; skipping",
            command.ExecutionId, command.ThroughStepIndex);

        return new WorkflowExecutionCompensated { ExecutionId = command.ExecutionId };
    }
}
