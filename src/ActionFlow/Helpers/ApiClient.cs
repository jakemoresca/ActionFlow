using ActionFlow.Domain.Actions;
using System.Text.Json.Nodes;

namespace ActionFlow.Helpers
{
	public class ApiClient(IHttpClientFactory httpClientFactory) : IApiClient
	{
		public async Task<ApiCallResult> CallGet(string url, Dictionary<string, string>? requestHeaders = null)
		{
			using var httpClient = httpClientFactory.CreateClient();
			AddRequestHeaders(requestHeaders, httpClient);

			var result = await httpClient.GetAsync(url);
			return await CreateApiCallResult(result);
		}

		public async Task<ApiCallResult> CallPost(string url, string data, Dictionary<string, string>? requestHeaders = null)
		{
			using var httpClient = httpClientFactory.CreateClient();
			AddRequestHeaders(requestHeaders, httpClient);

			var json = JsonNode.Parse(data);
			var content = new StringContent(json!.ToJsonString());
			var result = await httpClient.PostAsync(url, content);

			return await CreateApiCallResult(result);
		}

		private static void AddRequestHeaders(Dictionary<string, string>? requestHeaders, HttpClient httpClient)
		{
			if (requestHeaders != null)
			{
				foreach (var header in requestHeaders!)
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
