using ActionFlow.Domain.Engine;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ActionFlow.Engine.Providers.Extensions;
public static class WorkflowProviderBuiltinExtensions
{
	public static void UseFromJson(this IWorkflowProvider workflowProvider, JsonArray workflowsJson)
	{
		var workflows = workflowsJson.Deserialize<Workflow[]>();

		if (workflows != null)
		{
			foreach (var workflow in workflows)
			{
				workflowProvider.AddWorkflow(workflow);
			}
		}
	}

	public static void UseFromJson(this IWorkflowProvider workflowProvider, string jsonPath)
	{
		var jsonBytes = File.ReadAllBytes(jsonPath);
		var workflowsJson = JsonArray.Parse(jsonBytes)?.AsArray();

		workflowProvider.UseFromJson(workflowsJson!);
	}
}
