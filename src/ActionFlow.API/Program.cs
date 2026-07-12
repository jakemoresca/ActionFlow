using ActionFlow.Api.Endpoints;
using ActionFlow.Contracts;
using ActionFlow.DB.Extensions;
using ActionFlow.DB.Providers;
using ActionFlow.Engine.Providers;
using ActionFlow.Extensions;
using Confluent.Kafka;
using Marten;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wolverine;
using Wolverine.Kafka;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults: OpenTelemetry, health checks, service discovery.
builder.AddServiceDefaults();

// Connection strings injected by the Aspire AppHost; fallbacks for standalone / docker-compose.
var postgres = builder.Configuration.GetConnectionString("actionflow")
    ?? "Host=localhost;Port=5432;Database=actionflow;Username=actionflow;Password=actionflow";
var kafka = builder.Configuration.GetConnectionString("kafka") ?? "localhost:9092";

// Marten: shared workflow documents + the API-owned execution event streams and status projection.
builder.Services.AddMarten(options =>
{
    options.Connection(postgres);
    options.DatabaseSchemaName = "actionflow_api";
    options.UseSystemTextJsonForSerialization();
    options.ConfigureActionFlowStore();
    options.AddExecutionStatusProjection();
})
.IntegrateWithWolverine();

builder.UseWolverine(opts =>
{
    opts.UseRuntimeCompilation();
    opts.UseKafka(kafka)
        .AutoProvision()
        // Read from the start of the topic so the status/timeline consumer cannot miss execution
        // events emitted by the Runner before this consumer finishes its first group join. After the
        // first commit, Wolverine resumes from the committed offset, so events are not reprocessed.
        .ConfigureConsumers(config => config.AutoOffsetReset = AutoOffsetReset.Earliest);

    // Outbound (transactional outbox): execute commands + workflow-published events.
    opts.PublishMessage<ExecuteWorkflow>().ToKafkaTopic(Topics.ExecuteCommands);
    opts.PublishMessage<WorkflowPublished>().ToKafkaTopic(Topics.WorkflowEvents);

    // Inbound: execution events → appended to Marten streams for status/timeline.
    opts.ListenToKafkaTopic(Topics.ExecutionEvents);
});

// ActionFlow engine for synchronous test runs (POST /workflows/{name}/test). Resolves workflow
// definitions from the shared document store (latest version). Registered scoped so each request
// gets a fresh engine (no stale workflow cache after a save). The async Runner remains the path for
// real executions.
builder.Services.UseActionFlowEngine();
builder.Services.RemoveAll<IWorkflowProvider>();
builder.Services.AddScoped<IWorkflowProvider, DocumentStoreWorkflowProvider>();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    if (builder.Environment.IsDevelopment())
    {
        // The editor's origin is an Aspire-assigned dynamic port in dev; allow any origin.
        policy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod();
    }
    else
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"];
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    }
}));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();
app.MapWorkflowEndpoints();
app.MapExecutionEndpoints();

app.Run();

// Exposed for WebApplicationFactory-based tests.
public partial class Program;
