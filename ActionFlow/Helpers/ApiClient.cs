using ActionFlow.Domain;
using Newtonsoft.Json;
using System.Text.Json.Nodes;

namespace ActionFlow.Helpers
{
    public static class ApiClient
    {
        public static async Task<ApiCallResult> CallGet(string url, string? requestHeaders = null)
        {
            var httpClient = new HttpClient();
            AddRequestHeaders(requestHeaders, httpClient);

            var result = await httpClient.GetAsync(url);
            return await CreateApiCallResult(result);
        }

        public static async Task<ApiCallResult> CallPost(string url, string data, string? requestHeaders = null)
        {
            var httpClient = new HttpClient();
            AddRequestHeaders(requestHeaders, httpClient);

            var json = JsonNode.Parse(data);
            var content = new StringContent(json!.ToJsonString());
            var result = await httpClient.PostAsync(url, content);

            return await CreateApiCallResult(result);
        }

        private static void AddRequestHeaders(string? requestHeaders, HttpClient httpClient)
        {
            if (requestHeaders != null)
            {
                var headerDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestHeaders);
                foreach (var header in headerDictionary!)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        private static async Task<ApiCallResult> CreateApiCallResult(HttpResponseMessage result)
        {
            var jsonBody = JsonNode.Parse(await result.Content.ReadAsStringAsync());
            var headers = result.Headers.ToDictionary(x => x.Key, x => x.Value);
            var statusCode = (int)result.StatusCode;

            var apiCallResult = new ApiCallResult(jsonBody!, headers, statusCode);
            return apiCallResult;
        }
    }
}
