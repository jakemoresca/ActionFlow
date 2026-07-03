using ActionFlow.DB.Documents;
using ActionFlow.DB.Projections;
using ActionFlow.DB.Providers;
using ActionFlow.Engine.Providers;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Weasel.Core;

namespace ActionFlow.DB.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures Marten (document store + event store + execution projection) for ActionFlow and
    /// registers the <see cref="DocumentStoreWorkflowProvider"/> as the <see cref="IWorkflowProvider"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">PostgreSQL connection string.</param>
    /// <param name="registerAsWorkflowProvider">
    /// When true (default) registers the Marten-backed provider as the engine's <see cref="IWorkflowProvider"/>.
    /// </param>
    /// <param name="configure">Optional hook to further customize Marten's <see cref="StoreOptions"/>.</param>
    public static IServiceCollection AddActionFlowDb(
        this IServiceCollection services,
        string connectionString,
        bool registerAsWorkflowProvider = true,
        Action<StoreOptions>? configure = null)
    {
        // NOTE: the WorkflowExecutionStatus projection is registered Async (see ConfigureActionFlowStore),
        // so a host (ActionFlow.Runner / ActionFlow.API in later phases) must run the async daemon
        // via .AddAsyncDaemon(...) for the read model to update live. Tests rebuild it explicitly.
        services.AddMarten(options =>
        {
            options.Connection(connectionString);

            // Marten's default (Newtonsoft) leaves untyped Dictionary values as JToken; we rely on
            // System.Text.Json + JsonElement normalization in StepDocument for property fidelity.
            options.UseSystemTextJsonForSerialization();

            options.ConfigureActionFlowStore();

            configure?.Invoke(options);
        });

        if (registerAsWorkflowProvider)
        {
            services.AddScoped<IWorkflowProvider, DocumentStoreWorkflowProvider>();
        }

        return services;
    }

    /// <summary>
    /// Applies ActionFlow's document mappings, indexes and projections to a Marten
    /// <see cref="StoreOptions"/>. Shared so tests can build a store without the DI container.
    /// </summary>
    public static StoreOptions ConfigureActionFlowStore(this StoreOptions options)
    {
        options.Schema.For<WorkflowDocument>()
            .Identity(x => x.Id)
            .Duplicate(x => x.Name)
            .Duplicate(x => x.IsLatest);

        options.Projections.Add<WorkflowExecutionStatusProjection>(ProjectionLifecycle.Async);

        return options;
    }
}
