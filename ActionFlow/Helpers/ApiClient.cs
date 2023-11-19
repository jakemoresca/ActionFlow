using ActionFlow.Domain.Actions;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace ActionFlow.Helpers
{
    public class ApiClient : IApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ApiCallResult> CallGet(string url, Dictionary<string, string>? requestHeaders = null)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            AddRequestHeaders(requestHeaders, httpClient);

            var result = await httpClient.GetAsync(url);
            return await CreateApiCallResult(result);
        }

        public async Task<ApiCallResult> CallPost(string url, string data, Dictionary<string, string>? requestHeaders = null)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            AddRequestHeaders(requestHeaders, httpClient);

            var json = JsonNode.Parse(data);
            var content = new StringContent(json!.ToJsonString());
            var result = await httpClient.PostAsync(url, content);

            return await CreateApiCallResult(result);
        }

        private void AddRequestHeaders(Dictionary<string, string>? requestHeaders, HttpClient httpClient)
        {
            if (requestHeaders != null)
            {
                foreach (var header in requestHeaders!)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        private async Task<ApiCallResult> CreateApiCallResult(HttpResponseMessage result)
        {
            var jsonBody = JsonNode.Parse(await result.Content.ReadAsStringAsync());
            var headers = result.Headers.ToDictionary(x => x.Key, x => x.Value);
            var statusCode = (int)result.StatusCode;

            var apiCallResult = new ApiCallResult(jsonBody!, headers, statusCode);
            return apiCallResult;
        }
    }
}
