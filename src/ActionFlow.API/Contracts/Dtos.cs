namespace ActionFlow.Api.Contracts;

// ---- Design (workflow authoring) ----

/// <summary>A step in a workflow definition. <see cref="Properties"/> values are treated as expressions.</summary>
public record StepDto(
    string Name,
    string ActionType,
    string? ConditionExpression = null,
    Dictionary<string, object>? Properties = null);

/// <summary>A declared output parameter: <see cref="Name"/> = <see cref="Expression"/>.</summary>
public record ParameterDto(string Name, string Expression);

/// <summary>Request body for creating/publishing a workflow (POST /workflows or PUT /workflows/{name}).</summary>
public record WorkflowDefinitionRequest(
    string Name,
    List<StepDto> Steps,
    List<ParameterDto>? OutputParameters = null,
    Dictionary<string, string>? Metadata = null);

/// <summary>Full workflow definition returned by the design endpoints.</summary>
public record WorkflowResponse(
    string Name,
    int Version,
    bool IsLatest,
    List<StepDto> Steps,
    List<ParameterDto> OutputParameters,
    Dictionary<string, string> Metadata,
    DateTimeOffset CreatedAt);

/// <summary>Row in the workflow list (latest version per name).</summary>
public record WorkflowSummary(string Name, int LatestVersion, DateTimeOffset CreatedAt);

/// <summary>One published version of a workflow.</summary>
public record WorkflowVersionInfo(string Name, int Version, bool IsLatest, DateTimeOffset CreatedAt);

// ---- Validate ----

public record ValidationError(string Location, string Message);

public record ValidationResponse(bool IsValid, List<ValidationError> Errors);

// ---- Execute ----

/// <summary>Request body for POST /workflows/{name}/execute.</summary>
public record ExecuteRequest(
    Dictionary<string, string>? Inputs = null,
    int? Version = null);

public record ExecuteResponse(Guid ExecutionId);

// ---- Debug / query ----

public record ExecutionStatusResponse(
    Guid ExecutionId,
    string WorkflowName,
    int? Version,
    string Status,
    int CurrentStepIndex,
    List<string> CompletedSteps,
    string? Error,
    Dictionary<string, string> OutputParameters,
    DateTimeOffset RequestedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt);

/// <summary>One entry in an execution's event-stream timeline.</summary>
public record ExecutionEventRecord(
    long Sequence,
    int Version,
    DateTimeOffset Timestamp,
    string Type,
    object Data);
