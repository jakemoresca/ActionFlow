using ActionFlow.Domain.Engine;

namespace ActionFlow.Engine.Observers
{
	/// <summary>
	/// Lifecycle hook invoked by <see cref="StepExecutionEvaluator"/> around each executed step.
	/// Hosts (e.g. ActionFlow.Runner) implement this to emit <c>StepCompleted</c>/<c>StepFailed</c>
	/// events for the debugging timeline and fine-grained compensation. The default
	/// <see cref="NullStepExecutionObserver"/> is a no-op so the library's behavior is unchanged.
	/// Correlation (<see cref="ExecutionContext.ExecutionId"/>) and the current step index
	/// (<see cref="ExecutionContext.CurrentStepIndex"/>) are read from the execution context.
	/// Skipped steps (false condition) do not raise any callbacks.
	/// </summary>
	public interface IStepExecutionObserver
	{
		ValueTask OnStepStartingAsync(Step step, ExecutionContext context, CancellationToken cancellationToken = default);

		ValueTask OnStepCompletedAsync(Step step, ExecutionContext context, TimeSpan duration, CancellationToken cancellationToken = default);

		ValueTask OnStepFailedAsync(Step step, ExecutionContext context, Exception exception, CancellationToken cancellationToken = default);
	}

	/// <summary>No-op observer used when a host does not supply one. Preserves the original behavior.</summary>
	public sealed class NullStepExecutionObserver : IStepExecutionObserver
	{
		public static readonly NullStepExecutionObserver Instance = new();

		public ValueTask OnStepStartingAsync(Step step, ExecutionContext context, CancellationToken cancellationToken = default)
			=> ValueTask.CompletedTask;

		public ValueTask OnStepCompletedAsync(Step step, ExecutionContext context, TimeSpan duration, CancellationToken cancellationToken = default)
			=> ValueTask.CompletedTask;

		public ValueTask OnStepFailedAsync(Step step, ExecutionContext context, Exception exception, CancellationToken cancellationToken = default)
			=> ValueTask.CompletedTask;
	}
}
