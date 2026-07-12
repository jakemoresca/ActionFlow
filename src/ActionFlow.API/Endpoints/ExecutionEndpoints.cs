using ActionFlow.Api.Contracts;
using ActionFlow.Contracts;
using ActionFlow.DB.Documents;
using ActionFlow.DB.ReadModels;
using ActionFlow.Domain.Engine;
using ActionFlow.Engine;
using Marten;
using Wolverine.Marten;
using ExecutionContext = ActionFlow.Engine.ExecutionContext;

namespace ActionFlow.Api.Endpoints;

/// <summary>Execute + debug/query endpoints.</summary>
public static class ExecutionEndpoints
{
    public static IEndpointRouteBuilder MapExecutionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/workflows/{name}/execute", ExecuteAsync).WithTags("Execute");
        app.MapPost("/workflows/{name}/test", TestAsync).WithTags("Execute");

        var executions = app.MapGroup("/executions").WithTags("Executions");
        executions.MapGet("/", QueryAsync);
        executions.MapGet("/{id:guid}", GetStatusAsync);
        executions.MapGet("/{id:guid}/events", GetTimelineAsync);

        return app;
    }

    /// <summary>Appends <see cref="WorkflowExecutionRequested"/> to a new execution stream and publishes
    /// <see cref="ExecuteWorkflow"/> in the same Marten transaction (outbox); returns 202 + ExecutionId.</summary>
    private static async Task<IResult> ExecuteAsync(
        string name, ExecuteRequest? request, IDocumentSession session, IMartenOutbox outbox, CancellationToken ct)
    {
        request ??= new ExecuteRequest();

        var query = session.Query<WorkflowDocument>().Where(x => x.Name == name);
        var document = request.Version is int version
            ? await query.Where(x => x.Version == version).FirstOrDefaultAsync(ct)
            : await query.Where(x => x.IsLatest).FirstOrDefaultAsync(ct);

        if (document is null)
        {
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Workflow not found",
                detail: $"No {(request.Version is null ? "workflow" : $"version {request.Version} of workflow")} named '{name}'.");
        }

        var executionId = Guid.NewGuid();
        var inputs = request.Inputs ?? new Dictionary<string, string>();

        outbox.Enroll(session);

        session.Events.StartStream(executionId, new WorkflowExecutionRequested
        {
            ExecutionId = executionId,
            WorkflowName = name,
            Version = document.Version,
            RequestedAt = DateTimeOffset.UtcNow
        });

        await outbox.PublishAsync(new ExecuteWorkflow
        {
            ExecutionId = executionId,
            WorkflowName = name,
            Version = document.Version,
            Inputs = inputs,
            CorrelationId = executionId
        });

        await session.SaveChangesAsync(ct);

        return Results.Accepted($"/executions/{executionId}", new ExecuteResponse(executionId));
    }

    /// <summary>
    /// Runs a workflow synchronously in-process and returns its output immediately — the editor's
    /// "Test" path. Bypasses the async Runner/Kafka pipeline so the result is deterministic and
    /// available in the response (no polling of the execution read model).
    /// </summary>
    private static async Task<IResult> TestAsync(
        string name, ExecuteRequest? request, IActionFlowEngine engine)
    {
        request ??= new ExecuteRequest();

        var executionContext = new ExecutionContext(engine);
        foreach (var input in request.Inputs ?? new Dictionary<string, string>())
        {
            executionContext.AddOrUpdateParameter(new Parameter { Name = input.Key, Expression = input.Value });
        }

        try
        {
            var result = await engine.ExecuteWorkflowAsync(name, executionContext);
            var output = result.OutputParameters
                .ToDictionary(entry => entry.Key, entry => entry.Value?.ToString() ?? string.Empty);

            return Results.Ok(new TestRunResponse(true, output, null));
        }
        catch (KeyNotFoundException)
        {
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Workflow not found",
                detail: $"No workflow named '{name}'.");
        }
        catch (Exception exception)
        {
            // Surface engine/expression errors to the editor rather than a 500 — a failed test run is
            // a normal, inspectable outcome.
            return Results.Ok(new TestRunResponse(false, new Dictionary<string, string>(), exception.Message));
        }
    }

    private static async Task<IResult> GetStatusAsync(Guid id, IQuerySession session, CancellationToken ct)
    {
        var status = await session.LoadAsync<WorkflowExecutionStatus>(id, ct);
        return status is null ? NotFound(id) : Results.Ok(ToResponse(status));
    }

    private static async Task<IResult> GetTimelineAsync(Guid id, IQuerySession session, CancellationToken ct)
    {
        var events = await session.Events.FetchStreamAsync(id, token: ct);
        if (events.Count == 0)
        {
            return NotFound(id);
        }

        var timeline = events.Select(e => new ExecutionEventRecord(
            e.Sequence, (int)e.Version, e.Timestamp, e.EventTypeName, e.Data));

        return Results.Ok(timeline);
    }

    private static async Task<IResult> QueryAsync(string? status, string? workflow, IQuerySession session, CancellationToken ct)
    {
        IQueryable<WorkflowExecutionStatus> query = session.Query<WorkflowExecutionStatus>();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(workflow))
        {
            query = query.Where(x => x.WorkflowName == workflow);
        }

        var results = await query.ToListAsync(ct);
        return Results.Ok(results.Select(ToResponse));
    }

    private static ExecutionStatusResponse ToResponse(WorkflowExecutionStatus status) => new(
        status.Id, status.WorkflowName, status.Version, status.Status, status.CurrentStepIndex,
        status.CompletedSteps, status.Error, status.OutputParameters,
        status.RequestedAt, status.StartedAt, status.CompletedAt);

    private static IResult NotFound(Guid id) =>
        Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Execution not found", detail: $"No execution '{id}'.");
}
