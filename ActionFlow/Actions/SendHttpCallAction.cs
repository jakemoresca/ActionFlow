using ActionFlow.Domain.Actions;
using ActionFlow.Domain.Engine;
using ActionFlow.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionFlow.Actions
{
    public class SendHttpCallAction : ActionBase
    {
        public static string UrlKey  = "Url";
        public static string MethodKey = "Method";
        public static string HeadersKey = "Headers";
        public static string BodyKey = "Body";
        public static string ResultVariableKey = "ResultVariable";
        private readonly IApiClient _apiClient;

        public SendHttpCallAction(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public override async Task ExecuteAction()
        {
            var url = ExecutionContext!.GetActionProperty<string>(UrlKey);
            var method = ExecutionContext!.GetActionProperty<string>(MethodKey);
            var body = ExecutionContext!.GetActionProperty<string>(BodyKey);
            var headers = ExecutionContext!.GetActionProperty<Dictionary<string, string>>(HeadersKey);
            var resultVariable = ExecutionContext.GetActionProperty<string>(ResultVariableKey);

            ApiCallResult result;
            if (method == "GET")
            {
                result = await _apiClient.CallGet(url!, headers);
            }
            else if(method == "POST")
            {
                var bodyJson = JsonConvert.SerializeObject(body);
                result = await _apiClient.CallPost(url!, bodyJson, headers);
            }
            else
            {
                throw new InvalidOperationException($"Invalid method: {method}");
            }

            if(resultVariable != null)
            {
                ExecutionContext.AddOrUpdateParameter(resultVariable, result);
            }
        }
    }
}
