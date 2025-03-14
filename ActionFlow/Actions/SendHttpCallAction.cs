﻿using ActionFlow.Domain.Actions;
using ActionFlow.Helpers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ActionFlow.Actions
{
	public class SendHttpCallAction(IApiClient apiClient) : ActionBase
	{
		public readonly static string UrlKey = "Url";
		public readonly static string MethodKey = "Method";
		public readonly static string HeadersKey = "Headers";
		public readonly static string BodyKey = "Body";
		public readonly static string ResultVariableKey = "ResultVariable";

		public override string ActionType { get; } = "SendHttpCall";

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
				result = await apiClient.CallGet(url!, headers);
			}
			else if (method == "POST")
			{
                var bodyJson = JsonSerializer.Serialize(body);
                result = await apiClient.CallPost(url!, bodyJson, headers);
			}
			else
			{
				throw new InvalidOperationException($"Invalid method: {method}");
			}

			if (resultVariable != null)
			{
				ExecutionContext.AddOrUpdateParameter(resultVariable, result);
			}
		}
	}
}
