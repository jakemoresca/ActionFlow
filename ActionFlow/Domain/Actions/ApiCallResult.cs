using System.Text.Json.Nodes;

namespace ActionFlow.Domain.Actions
{
    public class ApiCallResult(JsonNode body, Dictionary<string, IEnumerable<string>> headers, int statusCode)
	{
		public JsonNode Body { get; } = body;
		public int StatusCode { get; } = statusCode;
		public Dictionary<string, IEnumerable<string>> Headers { get; } = headers;
	}
}
