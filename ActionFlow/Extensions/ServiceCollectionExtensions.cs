using ActionFlow.Actions;
using ActionFlow.Engine;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine.Providers;
using ActionFlow.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace ActionFlow.Extensions
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Register the default actions to the action flow engine. No need to call this if UseActionFlowEngine is used.
		/// </summary>
		public static void AddDefaultActions(this IServiceCollection services)
		{
			services.AddScoped<IActionBase, CallWorkflowAction>();
			services.AddScoped<IActionBase, ControlFlowAction>();
			services.AddScoped<IActionBase, ForLoopAction>();
			services.AddScoped<IActionBase, SendHttpCallAction>();
			services.AddScoped<IActionBase, SetVariableAction>();
		}

		/// <summary>
		/// Adds Action Flow engine and it's components to the service collection
		/// </summary>
		public static void UseActionFlowEngine(this IServiceCollection services)
		{
			services.AddScoped<IActionFlowEngine, ActionFlowEngine>();
			services.AddScoped<IWorkflowProvider, WorkflowProvider>();
			services.AddScoped<IHelperProvider, HelperProvider>();
			services.AddScoped<IStepExecutionEvaluator, StepExecutionEvaluator>();
			services.AddScoped<IStepActionFactory, StepActionFactory>();
			services.AddScoped<IApiClient, ApiClient>();
			services.AddHttpClient();

			services.AddDefaultActions();
		}
	}
}
