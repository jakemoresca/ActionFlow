using ActionFlow.Contracts;
using ActionFlow.DB.ReadModels;
using FluentAssertions;
using Marten;

namespace ActionFlow.DB.Tests;

[TestClass]
public class WorkflowExecutionStatusProjectionTests : MartenIntegrationTestBase
{
    [TestMethod]
    public async Task The_inline_projection_folds_the_execution_stream_into_a_status()
    {
        var executionId = Guid.NewGuid();

        await using (var session = Store.LightweightSession())
        {
            session.Events.StartStream(
                executionId,
                new WorkflowExecutionRequested
                {
                    ExecutionId = executionId,
                    WorkflowName = "Onboarding",
                    Version = 1,
                    RequestedAt = DateTimeOffset.UtcNow
                },
                new WorkflowExecutionStarted { ExecutionId = executionId, StartedAt = DateTimeOffset.UtcNow },
                new StepCompleted { ExecutionId = executionId, StepName = "initialize", Index = 0, DurationMs = 5 },
                new StepCompleted { ExecutionId = executionId, StepName = "check", Index = 1, DurationMs = 3 },
                new WorkflowExecutionCompleted
                {
                    ExecutionId = executionId,
                    OutputParameters = new Dictionary<string, string> { ["canVote"] = "true" }
                });

            // Inline projection: the read model is built as part of this commit.
            await session.SaveChangesAsync();
        }

        await using var query = Store.QuerySession();
        var status = await query.LoadAsync<WorkflowExecutionStatus>(executionId);

        status.Should().NotBeNull();
        status!.Status.Should().Be("Completed");
        status.WorkflowName.Should().Be("Onboarding");
        status.CompletedSteps.Should().Equal("initialize", "check");
        status.CurrentStepIndex.Should().Be(1);
        status.OutputParameters.Should().ContainKey("canVote").WhoseValue.Should().Be("true");
    }
}
