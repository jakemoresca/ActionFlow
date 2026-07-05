using ActionFlow.DB.Extensions;
using Marten;
using Testcontainers.PostgreSql;

namespace ActionFlow.DB.Tests;

/// <summary>
/// Base class for Marten integration tests. Spins up a throwaway PostgreSQL container via
/// Testcontainers and builds a <see cref="DocumentStore"/> configured exactly like production
/// (<see cref="ServiceCollectionExtensions.ConfigureActionFlowStore"/>).
///
/// When Docker is unavailable the test is marked inconclusive rather than failing, so the pure
/// unit tests in this assembly still run in environments without a container runtime.
/// </summary>
public abstract class MartenIntegrationTestBase
{
    private PostgreSqlContainer? _postgres;

    protected DocumentStore Store { get; private set; } = default!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        try
        {
            // Build() and StartAsync() both probe the Docker daemon; guard the whole thing so the
            // pure unit tests in this assembly still run where no container runtime is present.
            _postgres = new PostgreSqlBuilder("postgres:17").Build();
            await _postgres.StartAsync();
        }
        catch (Exception ex)
        {
            Assert.Inconclusive($"Docker/Testcontainers unavailable, skipping integration test: {ex.Message}");
        }

        Store = DocumentStore.For(options =>
        {
            options.Connection(_postgres!.GetConnectionString());
            options.UseSystemTextJsonForSerialization();
            options.ConfigureActionFlowStore();
            options.AddExecutionStatusProjection();
        });
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        Store?.Dispose();

        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }
    }
}
