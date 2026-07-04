var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL — Marten document + event store. Persistent so data survives AppHost restarts.
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgWeb();

var actionflowDb = postgres.AddDatabase("actionflow");

// Kafka broker with the bundled Kafka UI for browsing topics/messages.
var kafka = builder.AddKafka("kafka")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithKafkaUI();

// Component 3 — the Runner worker. Waits for its dependencies, and receives their connection
// strings (ConnectionStrings__actionflow, ConnectionStrings__kafka) via configuration.
builder.AddProject<Projects.ActionFlow_Runner>("runner")
    .WithReference(actionflowDb).WaitFor(actionflowDb)
    .WithReference(kafka).WaitFor(kafka);

builder.Build().Run();
