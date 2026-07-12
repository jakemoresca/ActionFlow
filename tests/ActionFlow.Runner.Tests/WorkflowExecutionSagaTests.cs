using ActionFlow.Contracts;
using ActionFlow.Runner.Sagas;
using FluentAssertions;

namespace ActionFlow.Runner.Tests;

/// <summary>
/// State-machine tests for <see cref="WorkflowExecutionSaga"/> covering the lifecycle transitions:
/// success, failure→compensate, timeout, and idempotent handling of duplicate terminal events.
/// </summary>
[TestClass]
public class WorkflowExecutionSagaTests
{
    private static ExecuteWorkflow NewCommand(Guid id) => new()
    {
        ExecutionId = id,
        WorkflowName = "Onboarding",
        Version = 2,
        Inputs = new Dictionary<string, string> { ["age"] = "20" }
    };

    [TestMethod]
    public void Start_puts_the_saga_running_and_cascades_started_run_and_timeout()
    {
        var id = Guid.NewGuid();

        var (saga, outgoing) = WorkflowExecutionSaga.Start(NewCommand(id));

        saga.Id.Should().Be(id);
        saga.WorkflowName.Should().Be("Onboarding");
        saga.WorkflowVersion.Should().Be(2);
        saga.Status.Should().Be(SagaStatus.Running);

        outgoing.OfType<WorkflowExecutionStarted>().Should().ContainSingle();
        outgoing.OfType<RunWorkflowExecution>().Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { ExecutionId = id, WorkflowName = "Onboarding", Version = 2 });
        outgoing.OfType<WorkflowExecutionTimeout>().Should().ContainSingle()
            .Which.ExecutionId.Should().Be(id);
    }

    [TestMethod]
    public void StepCompleted_records_progress()
    {
        var saga = RunningSaga(out var id);

        saga.Handle(new StepCompleted { ExecutionId = id, StepName = "init", Index = 0, DurationMs = 1 });
        saga.Handle(new StepCompleted { ExecutionId = id, StepName = "check", Index = 1, DurationMs = 1 });

        saga.CompletedSteps.Should().Equal("init", "check");
        saga.CurrentStepIndex.Should().Be(1);
    }

    [TestMethod]
    public void Completed_event_marks_the_saga_completed()
    {
        var saga = RunningSaga(out var id);

        saga.Handle(new WorkflowExecutionCompleted { ExecutionId = id });

        saga.Status.Should().Be(SagaStatus.Completed);
    }

    [TestMethod]
    public void Failure_with_completed_steps_starts_compensation()
    {
        var saga = RunningSaga(out var id);
        saga.Handle(new StepCompleted { ExecutionId = id, StepName = "charge", Index = 0, DurationMs = 1 });

        var cascade = saga.Handle(new WorkflowExecutionFailed { ExecutionId = id, Error = "kaboom", LastCompletedStepIndex = 0 });

        saga.Status.Should().Be(SagaStatus.Compensating);
        saga.Error.Should().Be("kaboom");
        cascade.Should().BeOfType<CompensateWorkflow>()
            .Which.Should().BeEquivalentTo(new { ExecutionId = id, ThroughStepIndex = 0 });
    }

    [TestMethod]
    public void Failure_with_no_completed_steps_fails_without_compensation()
    {
        var saga = RunningSaga(out var id);

        var cascade = saga.Handle(new WorkflowExecutionFailed { ExecutionId = id, Error = "kaboom", LastCompletedStepIndex = -1 });

        saga.Status.Should().Be(SagaStatus.Failed);
        cascade.Should().BeNull();
    }

    [TestMethod]
    public void Compensated_event_marks_the_saga_compensated()
    {
        var saga = RunningSaga(out var id);
        saga.Status = SagaStatus.Compensating;

        saga.Handle(new WorkflowExecutionCompensated { ExecutionId = id });

        saga.Status.Should().Be(SagaStatus.Compensated);
    }

    [TestMethod]
    public void Timeout_while_running_fails_the_execution()
    {
        var saga = RunningSaga(out var id);

        saga.Handle(new WorkflowExecutionTimeout(id));

        saga.Status.Should().Be(SagaStatus.Failed);
        saga.Error.Should().Contain("timed out");
    }

    [TestMethod]
    public void Timeout_after_completion_is_ignored()
    {
        var saga = RunningSaga(out var id);
        saga.Handle(new WorkflowExecutionCompleted { ExecutionId = id });

        saga.Handle(new WorkflowExecutionTimeout(id));

        saga.Status.Should().Be(SagaStatus.Completed);
    }

    [TestMethod]
    public void Duplicate_terminal_events_are_ignored_after_completion()
    {
        var saga = RunningSaga(out var id);
        saga.Handle(new WorkflowExecutionCompleted { ExecutionId = id });

        // A late/duplicate failure must not knock a completed execution into compensation.
        var cascade = saga.Handle(new WorkflowExecutionFailed { ExecutionId = id, Error = "late", LastCompletedStepIndex = 0 });

        saga.Status.Should().Be(SagaStatus.Completed);
        cascade.Should().BeNull();
    }

    private static WorkflowExecutionSaga RunningSaga(out Guid id)
    {
        id = Guid.NewGuid();
        var (saga, _) = WorkflowExecutionSaga.Start(NewCommand(id));
        return saga;
    }
}
