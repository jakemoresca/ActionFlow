using ActionFlow.Contracts;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using ActionFlow.Runner.Handlers;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Wolverine;
using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Runner.Tests;

[TestClass]
public class ExecuteWorkflowHandlerTests
{
    [TestMethod]
    public async Task On_success_it_publishes_started_and_completed_with_stringified_outputs()
    {
        var engine = Substitute.For<IActionFlowEngine>();
        engine.ExecuteWorkflowAsync(Arg.Any<string>(), Arg.Any<ExecutionContext>())
            .Returns(new ActionFlowEngineResult { OutputParameters = { ["canVote"] = true, ["age"] = 18 } });

        var bus = Substitute.For<IMessageBus>();
        var command = new ExecuteWorkflow
        {
            ExecutionId = Guid.NewGuid(),
            WorkflowName = "Onboarding",
            Inputs = new Dictionary<string, string> { ["age"] = "18" }
        };

        await new ExecuteWorkflowHandler().Handle(command, engine, new ExecutionEventBuffer(), bus, NullLogger<ExecuteWorkflowHandler>.Instance);

        await bus.Received(1).PublishAsync(
            Arg.Is<WorkflowExecutionStarted>(e => e.ExecutionId == command.ExecutionId),
            Arg.Any<DeliveryOptions>());
        await bus.Received(1).PublishAsync(
            Arg.Is<WorkflowExecutionCompleted>(e =>
                e.ExecutionId == command.ExecutionId &&
                e.OutputParameters["canVote"] == "True" &&
                e.OutputParameters["age"] == "18"),
            Arg.Any<DeliveryOptions>());
    }

    [TestMethod]
    public async Task On_failure_it_publishes_failed_and_does_not_throw()
    {
        var engine = Substitute.For<IActionFlowEngine>();
        engine.ExecuteWorkflowAsync(Arg.Any<string>(), Arg.Any<ExecutionContext>())
            .Throws(new InvalidOperationException("boom"));

        var bus = Substitute.For<IMessageBus>();
        var command = new ExecuteWorkflow { ExecutionId = Guid.NewGuid(), WorkflowName = "Onboarding" };

        await new ExecuteWorkflowHandler().Handle(command, engine, new ExecutionEventBuffer(), bus, NullLogger<ExecuteWorkflowHandler>.Instance);

        await bus.Received(1).PublishAsync(
            Arg.Is<WorkflowExecutionFailed>(e => e.ExecutionId == command.ExecutionId && e.Error == "boom"),
            Arg.Any<DeliveryOptions>());
        await bus.DidNotReceive().PublishAsync(Arg.Any<WorkflowExecutionCompleted>(), Arg.Any<DeliveryOptions>());
    }
}
