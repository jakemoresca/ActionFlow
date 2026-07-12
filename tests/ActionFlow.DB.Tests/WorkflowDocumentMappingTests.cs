using System.Text.Json;
using ActionFlow.DB.Documents;
using ActionFlow.Domain.Actions;
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

    [TestMethod]
    public void ToStep_normalizes_object_property_to_string_dictionary()
    {
        // A SetVariable step's "Variables" arrives from Marten as a JSON object; it must become a
        // Dictionary<string,string> so SetVariableAction can read it (not raw JSON text).
        var json = """
        {
            "Name": "initialize",
            "ActionType": "SetVariable",
            "ConditionExpression": null,
            "Properties": { "Variables": { "age": 42, "canWalk": true, "name": "\"Bob\"" } }
        }
        """;

        var stepDocument = JsonSerializer.Deserialize<StepDocument>(json)!;
        var step = stepDocument.ToStep();

        step.Properties!["Variables"].Should().BeOfType<Dictionary<string, string>>();
        var variables = (Dictionary<string, string>)step.Properties!["Variables"];
        variables["age"].Should().Be("42");
        variables["canWalk"].Should().Be("true");
        variables["name"].Should().Be("\"Bob\"");
    }

    [TestMethod]
    public void ToStep_reconstructs_control_flow_conditions_with_nested_steps()
    {
        // ControlFlow "Conditions" must become List<ScopedWorkflow>, with nested steps rebuilt (and
        // their own object properties normalized). Nested steps are camelCase as the editor emits them.
        var json = """
        {
            "Name": "check",
            "ActionType": "ControlFlow",
            "Properties": {
                "Conditions": [
                    {
                        "Expression": "canWalk == true",
                        "Steps": [
                            {
                                "name": "assign",
                                "actionType": "SetVariable",
                                "conditionExpression": null,
                                "properties": { "Variables": { "success": "true" } }
                            }
                        ]
                    }
                ]
            }
        }
        """;

        var step = JsonSerializer.Deserialize<StepDocument>(json)!.ToStep();

        step.Properties!["Conditions"].Should().BeOfType<List<ScopedWorkflow>>();
        var conditions = (List<ScopedWorkflow>)step.Properties!["Conditions"];
        conditions.Should().ContainSingle();
        conditions[0].Expression.Should().Be("canWalk == true");
        conditions[0].Steps.Should().ContainSingle();

        var nested = conditions[0].Steps![0];
        nested.ActionType.Should().Be("SetVariable");
        nested.Properties!["Variables"].Should().BeOfType<Dictionary<string, string>>();
        ((Dictionary<string, string>)nested.Properties!["Variables"])["success"].Should().Be("true");
    }

    [TestMethod]
    public void ToStep_reconstructs_for_loop_steps()
    {
        var json = """
        {
            "Name": "loop",
            "ActionType": "ForLoop",
            "Properties": {
                "InitializerVariable": "i",
                "InitialValue": "0",
                "Condition": "i < 3",
                "Iterator": "i + 1",
                "Steps": [
                    { "name": "inc", "actionType": "SetVariable", "properties": { "Variables": { "x": "i" } } }
                ]
            }
        }
        """;

        var step = JsonSerializer.Deserialize<StepDocument>(json)!.ToStep();

        step.Properties!["InitializerVariable"].Should().Be("i");
        step.Properties!["Steps"].Should().BeOfType<List<Step>>();
        var steps = (List<Step>)step.Properties!["Steps"];
        steps.Should().ContainSingle();
        steps[0].ActionType.Should().Be("SetVariable");
    }
}
