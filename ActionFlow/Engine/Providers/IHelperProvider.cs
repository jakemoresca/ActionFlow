using ActionFlow.Helpers;

namespace ActionFlow.Engine.Providers;
public interface IHelperProvider
{
	IApiClient GetApiClient();
}