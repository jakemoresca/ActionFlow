using ActionFlow.Helpers;

namespace ActionFlow.Engine.Providers;
public class HelperProvider(IApiClient apiClient) : IHelperProvider
{
	public IApiClient GetApiClient()
	{
		return apiClient;
	}
}
