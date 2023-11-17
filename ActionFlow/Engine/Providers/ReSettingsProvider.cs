using ActionFlow.Domain.Actions;
using ActionFlow.Helpers;
using RulesEngine.Models;

namespace ActionFlow.Engine.Providers
{
    public class ReSettingsProvider : IReSettingsProvider
    {
        public ReSettings GetReSettings()
        {
            var reSettingsWithCustomTypes = new ReSettings
            {
                CustomTypes = new Type[] { typeof(ApiClient), typeof(ApiCallResult) },
            };

            return reSettingsWithCustomTypes;
        }
    }
}
