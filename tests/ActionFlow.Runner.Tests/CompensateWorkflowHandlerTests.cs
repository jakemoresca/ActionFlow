using ActionFlow.Contracts;
using ActionFlow.Runner.Handlers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace ActionFlow.Runner.Tests;

[TestClass]
public class CompensateWorkflowHandlerTests
{
    [TestMethod]
    public void Handling_compensation_reports_compensated_for_the_same_execution()
    {
        var executionId = Guid.NewGuid();

        var result = new CompensateWorkflowHandler()
            .Handle(new CompensateWorkflow { ExecutionId = executionId, ThroughStepIndex = 3 },
                NullLogger<CompensateWorkflowHandler>.Instance);

        result.ExecutionId.Should().Be(executionId);
    }
}
