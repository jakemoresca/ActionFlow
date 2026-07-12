using ActionFlow.Contracts;
using ActionFlow.DB.Extensions;
using ActionFlow.DB.Providers;
using ActionFlow.Engine.Observers;
using ActionFlow.Engine.Providers;
using ActionFlow.Extensions;
using ActionFlow.Runner;
using ActionFlow.Runner.Observers;
using ActionFlow.Runner.Registry;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.Kafka;
using Wolverine.Marten;

var builder = Host.CreateApplicationBuilder(args);

// Aspire service defaults: OpenTelemetry (to the dashboard), health checks, service discovery.
builder.AddServiceDefaults();

// Connection strings are injected by the Aspire AppHost (ConnectionStrings__actionflow / __kafka);
// the fallbacks let the Runner also run standalone against deploy/docker-compose.yml.
var postgres = builder.Configuration.GetConnectionString("actionflow")
    ?? "Host=localhost;Port=5432;Database=actionflow;Username=actionflow;Password=actionflow";
var kafka = builder.Configuration.GetConnectionString("kafka") ?? "localhost:9092";

// Core ActionFlow engine + default actions (BlankWorkflowProvider + no-op observer are overridden below).
builder.Services.UseActionFlowEngine();

// Marten store + Wolverine transactional outbox. The Runner owns its schema (saga state, outbox);
// workflow definitions live in the shared schema so the API's writes are visible here. No read-side
// projection lives in the Runner, so no Marten async daemon is needed.
builder.Services.AddMarten(options =>
{
    options.Connection(postgres);
    options.DatabaseSchemaName = "actionflow_runner";
    options.UseSystemTextJsonForSerialization();
    options.ConfigureActionFlowStore();
})
.IntegrateWithWolverine();

// Marten-backed workflow provider (D4) — replaces the BlankWorkflowProvider (explicit RemoveAll,
// since Wolverine's container does not honor last-registration-wins).
builder.Services.RemoveAll<IWorkflowProvider>();
builder.Services.AddScoped<IWorkflowProvider, DocumentStoreWorkflowProvider>();

// Buffer step events during execution; the handler publishes them through its own outbox context.
// RemoveAll first: UseActionFlowEngine registers the no-op observer, and Wolverine's container does
// not honor last-registration-wins, so the replacement must be explicit.
builder.Services.AddScoped<ExecutionEventBuffer>();
builder.Services.RemoveAll<IStepExecutionObserver>();
builder.Services.AddScoped<IStepExecutionObserver, BufferingStepObserver>();

// Action registry (default actions come from UseActionFlowEngine's DI registrations).
builder.Services.AddScoped<IActionRegistry, ActionRegistry>();

builder.UseWolverine(opts =>
{
    opts.UseRuntimeCompilation();
    opts.UseKafka(kafka).AutoProvision();

    // Inbound: execution commands and workflow lifecycle events.
    opts.ListenToKafkaTopic(Topics.ExecuteCommands);
    opts.ListenToKafkaTopic(Topics.WorkflowEvents);

    // Outbound: all execution events onto the audit/debug stream.
    opts.PublishMessage<WorkflowExecutionStarted>().ToKafkaTopic(Topics.ExecutionEvents);
    opts.PublishMessage<StepCompleted>().ToKafkaTopic(Topics.ExecutionEvents);
    opts.PublishMessage<StepFailed>().ToKafkaTopic(Topics.ExecutionEvents);
    opts.PublishMessage<WorkflowExecutionCompleted>().ToKafkaTopic(Topics.ExecutionEvents);
    opts.PublishMessage<WorkflowExecutionFailed>().ToKafkaTopic(Topics.ExecutionEvents);
});

var host = builder.Build();
await host.RunAsync();
