using System.Text.Json.Nodes;

namespace ActionFlow.Domain
{
    public class ApiCallResult
    {
        public ApiCallResult(JsonNode body, Dictionary<string, IEnumerable<string>> headers, int statusCode)
        {
            Body = body;
            Headers = headers;
            StatusCode = statusCode;
        }

        public JsonNode Body { get; }
        public int StatusCode { get; }
        public Dictionary<string, IEnumerable<string>> Headers { get; } = new Dictionary<string, IEnumerable<string>>();
    }
}
