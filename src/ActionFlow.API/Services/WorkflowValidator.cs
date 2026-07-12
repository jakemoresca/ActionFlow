using System.Text.Json;
using ActionFlow.Api.Contracts;
using DynamicExpresso;

namespace ActionFlow.Api.Services;

/// <summary>
/// Surfaces Component 1's expression handling to catch malformed expressions before a workflow is
/// published or executed. Each expression is parsed with DynamicExpresso, with its referenced
/// identifiers declared as late-bound <c>object</c> parameters — so referencing a not-yet-known
/// workflow variable is fine (types are resolved at runtime), while genuine syntax errors
/// (unbalanced parentheses, trailing/invalid operators, empty operands) are reported.
/// </summary>
public static class WorkflowValidator
{
    public static ValidationResponse Validate(WorkflowDefinitionRequest request)
    {
        var errors = new List<ValidationError>();
        var interpreter = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LateBindObject);

        void Check(string location, string? expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return;
            }

            try
            {
                var identifiers = interpreter.DetectIdentifiers(expression);
                var parameters = identifiers.UnknownIdentifiers
                    .Select(name => new Parameter(name, typeof(object)))
                    .ToArray();

                interpreter.Parse(expression, parameters);
            }
            catch (Exception exception)
            {
                errors.Add(new ValidationError(location, exception.Message));
            }
        }

        for (var i = 0; i < request.Steps.Count; i++)
        {
            var step = request.Steps[i];
            Check($"steps[{i}].conditionExpression", step.ConditionExpression);

            foreach (var (key, value) in step.Properties ?? [])
            {
                // Only scalar string properties are expressions; nested structures (e.g. SetVariable's
                // Variables map) are action-specific and skipped here.
                var expression = value switch
                {
                    string s => s,
                    JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
                    _ => null
                };

                Check($"steps[{i}].properties.{key}", expression);
            }
        }

        for (var i = 0; i < (request.OutputParameters?.Count ?? 0); i++)
        {
            Check($"outputParameters[{i}].expression", request.OutputParameters![i].Expression);
        }

        return new ValidationResponse(errors.Count == 0, errors);
    }
}
