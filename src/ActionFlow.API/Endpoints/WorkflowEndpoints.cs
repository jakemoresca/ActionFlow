using ActionFlow.Api.Contracts;
using ActionFlow.Api.Services;
using ActionFlow.Contracts;
using ActionFlow.DB.Documents;
using ActionFlow.Domain.Engine;
using Marten;
using Wolverine.Marten;

namespace ActionFlow.Api.Endpoints;

/// <summary>Design (authoring) endpoints. Each publish writes a new <see cref="WorkflowDocument"/> version
/// and emits <see cref="WorkflowPublished"/> through the Marten/Wolverine outbox so Runners refresh.</summary>
public static class WorkflowEndpoints
{
    public static IEndpointRouteBuilder MapWorkflowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/workflows").WithTags("Workflows");

        group.MapGet("/", ListAsync);
        group.MapGet("/{name}", GetLatestAsync);
        group.MapGet("/{name}/versions", GetVersionsAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{name}", UpdateAsync);
        group.MapDelete("/{name}", DeleteAsync);
        group.MapPost("/validate", Validate);

        return app;
    }

    private static async Task<IResult> ListAsync(IQuerySession session, CancellationToken ct)
    {
        var latest = await session.Query<WorkflowDocument>().Where(x => x.IsLatest).ToListAsync(ct);
        var summaries = latest
            .OrderBy(x => x.Name)
            .Select(x => new WorkflowSummary(x.Name, x.Version, x.CreatedAt));

        return Results.Ok(summaries);
    }

    private static async Task<IResult> GetLatestAsync(string name, IQuerySession session, CancellationToken ct)
    {
        var doc = await session.Query<WorkflowDocument>()
            .Where(x => x.Name == name && x.IsLatest)
            .FirstOrDefaultAsync(ct);

        return doc is null ? NotFound(name) : Results.Ok(ToResponse(doc));
    }

    private static async Task<IResult> GetVersionsAsync(string name, IQuerySession session, CancellationToken ct)
    {
        var versions = await session.Query<WorkflowDocument>()
            .Where(x => x.Name == name)
            .OrderBy(x => x.Version)
            .ToListAsync(ct);

        if (versions.Count == 0)
        {
            return NotFound(name);
        }

        return Results.Ok(versions.Select(x => new WorkflowVersionInfo(x.Name, x.Version, x.IsLatest, x.CreatedAt)));
    }

    private static async Task<IResult> CreateAsync(WorkflowDefinitionRequest request, IDocumentSession session, IMartenOutbox outbox, CancellationToken ct)
    {
        var validation = WorkflowValidator.Validate(request);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(ToErrorDictionary(validation));
        }

        var response = await PublishVersionAsync(request, session, outbox, ct);
        return Results.Created($"/workflows/{response.Name}", response);
    }

    private static async Task<IResult> UpdateAsync(string name, WorkflowDefinitionRequest request, IDocumentSession session, IMartenOutbox outbox, CancellationToken ct)
    {
        // The route name is authoritative.
        var normalized = request with { Name = name };

        var validation = WorkflowValidator.Validate(normalized);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(ToErrorDictionary(validation));
        }

        var response = await PublishVersionAsync(normalized, session, outbox, ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteAsync(string name, IDocumentSession session, CancellationToken ct)
    {
        var exists = await session.Query<WorkflowDocument>().AnyAsync(x => x.Name == name, ct);
        if (!exists)
        {
            return NotFound(name);
        }

        session.DeleteWhere<WorkflowDocument>(x => x.Name == name);
        await session.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static IResult Validate(WorkflowDefinitionRequest request) => Results.Ok(WorkflowValidator.Validate(request));

    /// <summary>Stores the next version (demoting the current latest) and publishes WorkflowPublished
    /// in one Marten transaction via the outbox — no dual write.</summary>
    private static async Task<WorkflowResponse> PublishVersionAsync(
        WorkflowDefinitionRequest request, IDocumentSession session, IMartenOutbox outbox, CancellationToken ct)
    {
        var existing = await session.Query<WorkflowDocument>().Where(x => x.Name == request.Name).ToListAsync(ct);
        var nextVersion = existing.Count == 0 ? 1 : existing.Max(x => x.Version) + 1;

        foreach (var current in existing.Where(x => x.IsLatest))
        {
            current.IsLatest = false;
            session.Store(current);
        }

        var document = WorkflowDocument.FromWorkflow(ToWorkflow(request), nextVersion, isLatest: true, metadata: request.Metadata);
        session.Store(document);

        outbox.Enroll(session);
        await outbox.PublishAsync(new WorkflowPublished
        {
            Name = request.Name,
            Version = nextVersion,
            PublishedAt = DateTimeOffset.UtcNow
        });

        await session.SaveChangesAsync(ct);

        return ToResponse(document);
    }

    private static Workflow ToWorkflow(WorkflowDefinitionRequest request)
    {
        var steps = request.Steps
            .Select(s => new Step(s.Name, s.ActionType, s.Properties, s.ConditionExpression))
            .ToList();

        var outputs = request.OutputParameters?
            .Select(p => new Parameter { Name = p.Name, Expression = p.Expression })
            .ToList();

        return new Workflow(request.Name, steps, outputs);
    }

    private static WorkflowResponse ToResponse(WorkflowDocument document)
    {
        var steps = document.Steps
            .Select(s => new StepDto(s.Name, s.ActionType, s.ConditionExpression, s.Properties))
            .ToList();

        var outputs = (document.OutputParameters ?? [])
            .Select(p => new ParameterDto(p.Name ?? string.Empty, p.Expression ?? string.Empty))
            .ToList();

        return new WorkflowResponse(document.Name, document.Version, document.IsLatest, steps, outputs, document.Metadata, document.CreatedAt);
    }

    private static IResult NotFound(string name) =>
        Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Workflow not found", detail: $"No workflow named '{name}'.");

    private static IDictionary<string, string[]> ToErrorDictionary(ValidationResponse validation) =>
        validation.Errors
            .GroupBy(e => e.Location)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());
}
