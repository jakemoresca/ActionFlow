using System.Globalization;
using System.Text.Json;
using ActionFlow.Domain.Actions;
using ActionFlow.Domain.Engine;

namespace ActionFlow.Engine
{
    /// <summary>
    /// Reconstructs step <see cref="Step.Properties"/> from the loosely-typed form persisted by
    /// Marten / deserialized by System.Text.Json (values arrive as <see cref="JsonElement"/>) into
    /// the shapes the built-in actions expect:
    /// <list type="bullet">
    /// <item>scalars → string expressions;</item>
    /// <item>objects → <c>Dictionary&lt;string,string&gt;</c> (e.g. SetVariable <c>Variables</c>, SendHttpCall <c>Headers</c>);</item>
    /// <item><c>Steps</c> arrays → <c>List&lt;Step&gt;</c> (ForLoop body / nested ControlFlow steps);</item>
    /// <item><c>Conditions</c> arrays → <c>List&lt;ScopedWorkflow&gt;</c> (ControlFlow branches);</item>
    /// <item>other arrays → raw JSON text.</item>
    /// </list>
    /// Property/field names are matched case-insensitively (top-level keys are PascalCase, while
    /// nested step objects authored by the editor are camelCase). Recurses into nested steps so a
    /// control-flow branch can itself contain variable/HTTP/loop/control-flow actions.
    /// </summary>
    public static class StepPropertyNormalizer
    {
        private const string StepsKey = "Steps";
        private const string ConditionsKey = "Conditions";

        public static Dictionary<string, object> Normalize(IDictionary<string, object> properties)
        {
            var normalized = new Dictionary<string, object>();
            foreach (var property in properties)
            {
                normalized[property.Key] = NormalizeValue(property.Key, property.Value);
            }
            return normalized;
        }

        private static object NormalizeValue(string key, object value) =>
            value is JsonElement element ? NormalizeElement(key, element) : value;

        private static object NormalizeElement(string key, JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                if (KeyIs(key, StepsKey)) return DeserializeSteps(element);
                if (KeyIs(key, ConditionsKey)) return DeserializeScopedWorkflows(element);
                // Other arrays are kept as raw JSON text (the engine treats them as string expressions).
                return element.GetRawText();
            }

            return element.ValueKind switch
            {
                JsonValueKind.Object => element.EnumerateObject()
                    .ToDictionary(property => property.Name, property => Scalar(property.Value)),
                _ => Scalar(element)
            };
        }

        private static List<Step> DeserializeSteps(JsonElement array)
        {
            var steps = new List<Step>();
            foreach (var item in array.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    steps.Add(DeserializeStep(item));
                }
            }
            return steps;
        }

        private static Step DeserializeStep(JsonElement element)
        {
            var name = GetProperty(element, "Name")?.GetString() ?? string.Empty;
            var actionType = GetProperty(element, "ActionType")?.GetString() ?? string.Empty;

            var conditionElement = GetProperty(element, "ConditionExpression");
            var condition = conditionElement is { ValueKind: JsonValueKind.String } c
                ? c.GetString()
                : null;

            var properties = new Dictionary<string, object>();
            if (GetProperty(element, "Properties") is { ValueKind: JsonValueKind.Object } propertiesElement)
            {
                foreach (var property in propertiesElement.EnumerateObject())
                {
                    properties[property.Name] = NormalizeElement(property.Name, property.Value);
                }
            }

            return new Step(name, actionType, properties, condition);
        }

        private static List<ScopedWorkflow> DeserializeScopedWorkflows(JsonElement array)
        {
            var scopedWorkflows = new List<ScopedWorkflow>();
            foreach (var item in array.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) continue;

                var stepsElement = GetProperty(item, StepsKey);
                scopedWorkflows.Add(new ScopedWorkflow
                {
                    Expression = GetProperty(item, "Expression")?.GetString(),
                    Steps = stepsElement is { ValueKind: JsonValueKind.Array } steps
                        ? DeserializeSteps(steps)
                        : new List<Step>()
                });
            }
            return scopedWorkflows;
        }

        private static JsonElement? GetProperty(JsonElement element, string name)
        {
            if (element.ValueKind != JsonValueKind.Object) return null;
            foreach (var property in element.EnumerateObject())
            {
                if (KeyIs(property.Name, name)) return property.Value;
            }
            return null;
        }

        private static bool KeyIs(string key, string expected) =>
            string.Equals(key, expected, StringComparison.OrdinalIgnoreCase);

        private static string Scalar(JsonElement element) => element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.TryGetInt64(out var l)
                ? l.ToString(CultureInfo.InvariantCulture)
                : element.GetDouble().ToString(CultureInfo.InvariantCulture),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => string.Empty,
            // Nested objects/arrays inside a value we treat as scalar fall back to raw JSON text.
            _ => element.GetRawText()
        };
    }
}
