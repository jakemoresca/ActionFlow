global using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;

namespace ActionFlow.Api.Tests;

/// <summary>
/// Boots the real API <c>Program</c> against a Testcontainers PostgreSQL, with Wolverine's external
/// (Kafka) transports stubbed so no broker is required. Marten writes/reads and the inline execution
/// projection run for real; published messages go to Wolverine's in-memory stub.
/// </summary>
public sealed class ApiTestFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.UseSetting("ConnectionStrings:actionflow", connectionString);
        builder.UseSetting("ConnectionStrings:kafka", "localhost:9092");

        builder.ConfigureServices(services => services.DisableAllExternalWolverineTransports());
    }
}
