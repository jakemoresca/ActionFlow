using ActionFlow.DB.Documents;
using ActionFlow.DB.Projections;
using ActionFlow.DB.Providers;
using ActionFlow.Engine.Providers;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace ActionFlow.DB.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// PostgreSQL schema that holds the shared <see cref="WorkflowDocument"/> table. Placing workflow
    /// definitions in a dedicated schema (separate from each service's default schema) lets the API
    /// own writes while the Runner reads them, without the two services sharing their Wolverine
    /// transport / saga / projection tables.
    /// </summary>
    public const string WorkflowSchemaName = "actionflow_workflows";

    /// <summary>
    /// One-call setup for a single-service host: Marten (shared workflow docs + read-side execution
    /// projection) plus the Marten-backed <see cref="IWorkflowProvider"/>. Multi-service hosts
    /// (Runner/API) compose Marten themselves and pick the pieces they need.
    /// </summary>
    public static IServiceCollection AddActionFlowDb(
        this IServiceCollection services,
        string connectionString,
        bool registerAsWorkflowProvider = true,
        Action<StoreOptions>? configure = null)
    {
        services.AddMarten(options =>
        {
            options.Connection(connectionString);
            options.UseSystemTextJsonForSerialization();
            options.ConfigureActionFlowStore();
            options.AddExecutionStatusProjection();
            configure?.Invoke(options);
        });

        if (registerAsWorkflowProvider)
        {
            services.AddScoped<IWorkflowProvider, DocumentStoreWorkflowProvider>();
        }

        return services;
    }

    /// <summary>
    /// Maps the shared <see cref="WorkflowDocument"/> (schema, identity, indexes). Both the API and the
    /// Runner call this so they see the same workflow definitions. Marten's default (Newtonsoft) leaves
    /// untyped dictionary values as JToken, so callers should also select System.Text.Json — see
    /// <c>StepDocument</c> normalization.
    /// </summary>
    public static StoreOptions ConfigureActionFlowStore(this StoreOptions options)
    {
        options.Schema.For<WorkflowDocument>()
            .DatabaseSchemaName(WorkflowSchemaName)
            .Identity(x => x.Id)
            .Duplicate(x => x.Name)
            .Duplicate(x => x.IsLatest);

        return options;
    }

    /// <summary>
    /// Registers the read-side <see cref="WorkflowExecutionStatusProjection"/>. Only the service that
    /// owns the execution event streams + debugging read models (the API) should add this. Registered
    /// Inline so the status is immediately consistent with the appended events (no async daemon needed).
    /// </summary>
    public static StoreOptions AddExecutionStatusProjection(this StoreOptions options)
    {
        options.Projections.Add<WorkflowExecutionStatusProjection>(ProjectionLifecycle.Inline);
        return options;
    }
}
