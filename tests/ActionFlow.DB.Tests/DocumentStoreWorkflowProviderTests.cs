using ActionFlow.DB.Documents;
using ActionFlow.DB.Providers;
using ActionFlow.Domain.Engine;
using FluentAssertions;
using Marten;

namespace ActionFlow.DB.Tests;

[TestClass]
public class DocumentStoreWorkflowProviderTests : MartenIntegrationTestBase
{
    [TestMethod]
    public async Task GetWorkflowAsync_returns_the_latest_version_by_name()
    {
        await StoreWorkflowVersion("Billing", version: 1, isLatest: false, marker: "v1");
        await StoreWorkflowVersion("Billing", version: 2, isLatest: true, marker: "v2");

        var provider = new DocumentStoreWorkflowProvider(Store);

        var workflow = await provider.GetWorkflowAsync("Billing");

        workflow.Should().NotBeNull();
        workflow!.Steps.Should().ContainSingle().Which.Name.Should().Be("v2");
    }

    [TestMethod]
    public async Task GetWorkflowAsync_by_version_returns_the_pinned_version()
    {
        await StoreWorkflowVersion("Billing", version: 1, isLatest: false, marker: "v1");
        await StoreWorkflowVersion("Billing", version: 2, isLatest: true, marker: "v2");

        var provider = new DocumentStoreWorkflowProvider(Store);

        var workflow = await provider.GetWorkflowAsync("Billing", version: 1);

        workflow.Should().NotBeNull();
        workflow!.Steps.Should().ContainSingle().Which.Name.Should().Be("v1");
    }

    [TestMethod]
    public async Task GetWorkflowAsync_returns_null_when_missing()
    {
        var provider = new DocumentStoreWorkflowProvider(Store);

        var workflow = await provider.GetWorkflowAsync("does-not-exist");

        workflow.Should().BeNull();
    }

    [TestMethod]
    public async Task Stored_workflow_round_trips_steps_properties_and_outputs()
    {
        var original = new Workflow(
            "Onboarding",
            [
                new("initialize", "Variable", new Dictionary<string, object>
                {
                    { "age", "18" },
                    { "canWalk", "true" }
                })
            ],
            [
                new Parameter { Name = "canVote", Expression = "age >= 18 && canWalk == true" }
            ]);

        await using (var session = Store.LightweightSession())
        {
            session.Store(WorkflowDocument.FromWorkflow(original, version: 1));
            await session.SaveChangesAsync();
        }

        var provider = new DocumentStoreWorkflowProvider(Store);
        var loaded = await provider.GetWorkflowAsync("Onboarding");

        loaded.Should().NotBeNull();
        loaded!.Steps.Should().ContainSingle();
        loaded.Steps[0].Properties!["age"].Should().Be("18");
        loaded.Steps[0].Properties!["canWalk"].Should().Be("true");
        loaded.OutputParameters.Should().ContainSingle()
            .Which.Expression.Should().Be("age >= 18 && canWalk == true");
    }

    private async Task StoreWorkflowVersion(string name, int version, bool isLatest, string marker)
    {
        var workflow = new Workflow(name, [new Step(marker, "Variable")]);
        await using var session = Store.LightweightSession();
        session.Store(WorkflowDocument.FromWorkflow(workflow, version, isLatest));
        await session.SaveChangesAsync();
    }
}
