using ActionFlow.Actions;
using ActionFlow.Contracts;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine.Observers;
using ActionFlow.Engine.Providers;
using ActionFlow.Extensions;
using ActionFlow.Runner.Handlers;
using ActionFlow.Runner.Observers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Wolverine;
using Wolverine.Tracking;

namespace ActionFlow.Runner.Tests;

/// <summary>
/// Phase 2 exit criterion: publish <see cref="ExecuteWorkflow"/> → the hosted engine runs the
/// workflow → <see cref="WorkflowExecutionCompleted"/> (and per-step <see cref="StepCompleted"/>)
/// are emitted. Uses Wolverine's tracked in-memory invocation to exercise the real handler + engine
/// + observer pipeline (Kafka transport is verified separately in the integration suite).
/// </summary>
[TestClass]
public class ExecuteWorkflowEndToEndTests
{
    [TestMethod]
    public async Task Invoking_ExecuteWorkflow_runs_engine_and_emits_execution_events()
    {
        var provider = Substitute.For<IWorkflowProvider>();
        provider.GetWorkflowAsync("Onboarding", Arg.Any<CancellationToken>())
            .Returns(new Workflow(
                "Onboarding",
                [
                    new("set canVote", "SetVariable", new Dictionary<string, object>
                    {
                        {
                            SetVariableAction.VariablesKey,
                            new Dictionary<string, string> { { "canVote", "age >= 18" } }
                        }
                    })
                ],
                [new Parameter { Name = "canVote", Expression = "canVote" }]));

        using var host = await Host.CreateDefaultBuilder()
            .UseWolverine(opts =>
            {
                opts.UseRuntimeCompilation();
                opts.Services.UseActionFlowEngine();
                opts.Services.AddSingleton(provider);
                opts.Services.AddScoped<ExecutionEventBuffer>();
                opts.Services.RemoveAll<IStepExecutionObserver>();
                opts.Services.AddScoped<IStepExecutionObserver, BufferingStepObserver>();
                opts.Discovery.IncludeAssembly(typeof(RunWorkflowExecutionHandler).Assembly);
            })
            .StartAsync();

        var command = new ExecuteWorkflow
        {
            ExecutionId = Guid.NewGuid(),
            WorkflowName = "Onboarding",
            Inputs = new Dictionary<string, string> { ["age"] = "20" }
        };

        // WorkflowExecutionStarted has no local consumer in this test host (it goes to Kafka in prod),
        // so relax the default no-exceptions assertion and verify the flow via explicit assertions.
        var session = await host.TrackActivity()
            .DoNotAssertOnExceptionsDetected()
            .PublishMessageAndWaitAsync(command);

        // The saga also consumes these events, so a message can appear in more than one envelope
        // record (sent + received); assert on presence + content rather than an exact count.
        var completed = session.FindEnvelopesWithMessageType<WorkflowExecutionCompleted>()
            .Select(record => record.Envelope?.Message).OfType<WorkflowExecutionCompleted>().ToList();
        completed.Should().NotBeEmpty();
        completed[0].OutputParameters["canVote"].Should().Be("True");

        var steps = session.FindEnvelopesWithMessageType<StepCompleted>()
            .Select(record => record.Envelope?.Message).OfType<StepCompleted>().ToList();
        steps.Should().NotBeEmpty();
        steps[0].StepName.Should().Be("set canVote");
    }
}
