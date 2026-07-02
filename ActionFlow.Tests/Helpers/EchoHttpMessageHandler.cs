using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ActionFlow.Tests.Helpers
{
    /// <summary>
    /// A stub <see cref="HttpMessageHandler"/> that simulates the subset of
    /// httpbin.org's echo behaviour the HTTP tests rely on. It always responds
    /// with 200 OK and a JSON body echoing the request's url, method, headers
    /// and parsed body, so the tests run deterministically without network access.
    /// </summary>
    public class EchoHttpMessageHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var headers = new JsonObject();
            foreach (var header in request.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }

            JsonNode? json = null;
            if (request.Content != null)
            {
                var body = await request.Content.ReadAsStringAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(body))
                {
                    try
                    {
                        json = JsonNode.Parse(body);
                    }
                    catch (JsonException)
                    {
                        json = JsonValue.Create(body);
                    }
                }
            }

            var responseBody = new JsonObject
            {
                ["url"] = request.RequestUri?.ToString(),
                ["method"] = request.Method.Method,
                ["headers"] = headers,
                ["json"] = json,
            };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody.ToJsonString()),
            };
        }
    }
}
