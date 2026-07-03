using System.Globalization;
using System.Text.Json;
using ActionFlow.Domain.Engine;

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

    public Workflow ToWorkflow()
    {
        var steps = Steps.Select(s => s.ToStep()).ToList();
        var outputs = OutputParameters?
            .Select(p => new Parameter { Name = p.Name, Expression = p.Expression })
            .ToList();

        return new Workflow(Name, steps, outputs);
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
        var properties = Properties.ToDictionary(kvp => kvp.Key, kvp => Normalize(kvp.Value));
        return new Step(Name, ActionType, properties, ConditionExpression);
    }

    /// <summary>
    /// After a JSON round-trip (System.Text.Json), property values come back as <see cref="JsonElement"/>.
    /// Coerce them to strings to match the engine's "every property value is a string expression"
    /// contract and the <c>ObjectToStringConverter</c> used by the JSON provider.
    /// </summary>
    private static object Normalize(object value) => value switch
    {
        JsonElement element => NormalizeElement(element),
        _ => value
    };

    private static object NormalizeElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString() ?? string.Empty,
        JsonValueKind.Number => element.TryGetInt64(out var l)
            ? l.ToString(CultureInfo.InvariantCulture)
            : element.GetDouble().ToString(CultureInfo.InvariantCulture),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Null => string.Empty,
        // Objects/arrays are kept as their raw JSON text, mirroring ObjectToStringConverter.
        _ => element.GetRawText()
    };
}

public class ParameterDocument
{
    public string? Name { get; set; }
    public string? Expression { get; set; }
}
