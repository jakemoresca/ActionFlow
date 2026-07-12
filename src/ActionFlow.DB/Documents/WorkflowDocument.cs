using ActionFlow.Domain.Engine;
using ActionFlow.Engine;

namespace ActionFlow.DB.Documents;

/// <summary>
/// Marten document form of a core <see cref="Workflow"/>, adding identity + versioning metadata.
/// The document is versioned: each publish is a new document (<c>Name:Version</c>) and exactly one
/// version per name is flagged <see cref="IsLatest"/>.
/// </summary>
public class WorkflowDocument
{
    /// <summary>Composite identity: <c>{Name}:{Version}</c>.</summary>
    public string Id { get; set; } = default!;

    public string Name { get; set; } = default!;
    public int Version { get; set; }
    public bool IsLatest { get; set; }

    public List<StepDocument> Steps { get; set; } = new();
    public List<ParameterDocument>? OutputParameters { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }

    public static string MakeId(string name, int version) => $"{name}:{version}";

    public static WorkflowDocument FromWorkflow(
        Workflow workflow,
        int version,
        bool isLatest = true,
        Dictionary<string, string>? metadata = null,
        DateTimeOffset? createdAt = null)
    {
        return new WorkflowDocument
        {
            Id = MakeId(workflow.WorkflowName, version),
            Name = workflow.WorkflowName,
            Version = version,
            IsLatest = isLatest,
            Steps = workflow.Steps.Select(StepDocument.FromStep).ToList(),
            OutputParameters = workflow.OutputParameters?
                .Select(p => new ParameterDocument { Name = p.Name, Expression = p.Expression })
                .ToList(),
            Metadata = metadata ?? new Dictionary<string, string>(),
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow
        };
    }

    /// <summary>Metadata flag opting the workflow into emitting the entire final context as output.</summary>
    public const string OutputAllMetadataKey = "outputAll";

    public Workflow ToWorkflow()
    {
        var steps = Steps.Select(s => s.ToStep()).ToList();
        var outputs = OutputParameters?
            .Select(p => new Parameter { Name = p.Name, Expression = p.Expression })
            .ToList();

        var outputAll = Metadata.TryGetValue(OutputAllMetadataKey, out var flag)
            && string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase);

        return new Workflow(Name, steps, outputs) { OutputAllParameters = outputAll };
    }
}

public class StepDocument
{
    public string Name { get; set; } = default!;
    public string ActionType { get; set; } = default!;
    public string? ConditionExpression { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();

    public static StepDocument FromStep(Step step) => new()
    {
        Name = step.Name,
        ActionType = step.ActionType,
        ConditionExpression = step.ConditionExpression,
        Properties = step.Properties ?? new Dictionary<string, object>()
    };

    public Step ToStep()
    {
        // Normalization (JsonElement -> string / Dictionary / nested Step / ScopedWorkflow) is shared
        // with the engine so persisted and file-based workflows resolve to the same shapes.
        var properties = StepPropertyNormalizer.Normalize(Properties);
        return new Step(Name, ActionType, properties, ConditionExpression);
    }
}

public class ParameterDocument
{
    public string? Name { get; set; }
    public string? Expression { get; set; }
}
