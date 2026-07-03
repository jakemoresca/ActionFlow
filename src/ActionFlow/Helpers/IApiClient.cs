using ActionFlow.Domain.Actions;

namespace ActionFlow.Helpers
{
    public interface IApiClient
    {
        Task<ApiCallResult> CallGet(string url, Dictionary<string, string>? requestHeaders = null);
        Task<ApiCallResult> CallPost(string url, string data, Dictionary<string, string>? requestHeaders = null);
    }
}