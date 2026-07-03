using System.Text.Json;
using ActionFlow.DB.Documents;
using ActionFlow.Domain.Engine;
using FluentAssertions;

namespace ActionFlow.DB.Tests;

/// <summary>
/// Pure mapping tests for <see cref="WorkflowDocument"/> — no database required.
/// Verifies the core <see cref="Workflow"/> ↔ document round-trip and JSON property normalization.
/// </summary>
[TestClass]
public class WorkflowDocumentMappingTests
{
    [TestMethod]
    public void FromWorkflow_then_ToWorkflow_preserves_shape()
    {
        var workflow = new Workflow(
            "Onboarding",
            [
                new("initialize", "Variable", new Dictionary<string, object>
                {
                    { "age", "1" },
                    { "canWalk", "true" }
                }),
                new("check", "Variable", [], "age == 1 && canWalk == true")
            ],
            [
                new Parameter { Name = "canVote", Expression = "age >= 18 && canWalk == true" }
            ]);

        var document = WorkflowDocument.FromWorkflow(workflow, version: 3);
        var roundTripped = document.ToWorkflow();

        document.Id.Should().Be("Onboarding:3");
        document.Name.Should().Be("Onboarding");
        document.Version.Should().Be(3);
        document.IsLatest.Should().BeTrue();

        roundTripped.WorkflowName.Should().Be("Onboarding");
        roundTripped.Steps.Should().HaveCount(2);
        roundTripped.Steps[0].Properties.Should().ContainKey("age").WhoseValue.Should().Be("1");
        roundTripped.Steps[1].ConditionExpression.Should().Be("age == 1 && canWalk == true");
        roundTripped.OutputParameters.Should().ContainSingle()
            .Which.Expression.Should().Be("age >= 18 && canWalk == true");
    }

    [TestMethod]
    public void ToStep_normalizes_JsonElement_property_values_to_strings()
    {
        // Simulate what Marten hands back after a System.Text.Json round-trip: values are JsonElements.
        var json = """
        {
            "Name": "initialize",
            "ActionType": "Variable",
            "ConditionExpression": null,
            "Properties": { "age": 42, "flag": true, "label": "hello" }
        }
        """;

        var stepDocument = JsonSerializer.Deserialize<StepDocument>(json)!;
        var step = stepDocument.ToStep();

        step.Properties!["age"].Should().Be("42");
        step.Properties!["flag"].Should().Be("true");
        step.Properties!["label"].Should().Be("hello");
        step.Properties!.Values.Should().AllBeOfType<string>();
    }
}
