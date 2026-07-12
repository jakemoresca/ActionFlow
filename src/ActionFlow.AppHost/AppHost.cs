var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL — Marten document + event store. Persistent so data survives AppHost restarts.
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Session)
    .WithPgWeb();

var actionflowDb = postgres.AddDatabase("actionflow");

// Kafka broker with the bundled Kafka UI for browsing topics/messages.
var kafka = builder.AddKafka("kafka")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Session)
    .WithKafkaUI();

// Component 3 — the Runner worker. Waits for its dependencies, and receives their connection
// strings (ConnectionStrings__actionflow, ConnectionStrings__kafka) via configuration.
builder.AddProject<Projects.ActionFlow_Runner>("runner")
    .WithReference(actionflowDb).WaitFor(actionflowDb)
    .WithReference(kafka).WaitFor(kafka);

// Component 4 — the HTTP API / editor backend. The "http" launch profile defines its http endpoint.
var api = builder.AddProject<Projects.ActionFlow_API>("api", "http")
    .WithReference(actionflowDb).WaitFor(actionflowDb)
    .WithReference(kafka).WaitFor(kafka)
    .WithExternalHttpEndpoints();

// Component 2 — the Next.js visual editor. Runs `npm run dev`; Aspire assigns its port via PORT and
// passes the API base URL through so the editor can call it once wired up. It is not gated on API
// health (a frontend should start and surface errors rather than block on the backend).
builder.AddJavaScriptApp("editor", "../../actionFlow.editor")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WithEnvironment("NEXT_PUBLIC_ACTIONFLOW_API", api.GetEndpoint("http"));

builder.Build().Run();
