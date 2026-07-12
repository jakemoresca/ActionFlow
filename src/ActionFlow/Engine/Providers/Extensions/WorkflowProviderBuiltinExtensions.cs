using ActionFlow.Domain.Engine;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ActionFlow.Engine.Providers.Extensions;
public static class WorkflowProviderBuiltinExtensions
{
	public static void AddJsonWorkflowProvider(this IServiceCollection services, string jsonPath)
	{
		var jsonString = File.ReadAllText(jsonPath);
		var workflowsJson = JsonArray.Parse(jsonString)?.AsArray();

		services.AddJsonWorkflowProvider(workflowsJson!);
	}

	public static void AddJsonWorkflowProvider(this IServiceCollection services, JsonArray workflowsJson)
	{
		var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		options.Converters.Add(new ObjectToStringConverter());

		var workflows = workflowsJson.Deserialize<List<Workflow>>(options);

		if (workflows != null)
		{
			services.AddScoped<IWorkflowProvider>(x =>
			{
				return new WorkflowProvider([.. workflows]);
			});
		}
	}

	public class ObjectToStringConverter : JsonConverter<object>
	{
		public override bool CanConvert(Type typeToConvert) => typeof(object) == typeToConvert;

		public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Number)
			{
				return reader.TryGetInt64(out long l) ? l.ToString() : reader.GetDouble().ToString();
			}
			if (reader.TokenType == JsonTokenType.String)
			{
				return reader.GetString() ?? string.Empty;
			}
			using (JsonDocument document = JsonDocument.ParseValue(ref reader))
			{
				var root = document.RootElement.Clone();

				// Objects become a string->string dictionary so dictionary-typed action
				// properties (e.g. SetVariable "Variables", SendHttpCall "Headers") are usable
				// rather than raw JSON text. Each value stays a string expression.
				if (root.ValueKind == JsonValueKind.Object)
				{
					return root.EnumerateObject()
						.ToDictionary(property => property.Name, property => property.Value.ToString());
				}

				return root.ToString();
			}
		}

		public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
