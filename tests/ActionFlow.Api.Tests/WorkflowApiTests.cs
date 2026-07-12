using System.Net;
using System.Net.Http.Json;
using ActionFlow.Api.Contracts;
using FluentAssertions;
using Testcontainers.PostgreSql;

namespace ActionFlow.Api.Tests;

[TestClass]
public class WorkflowApiTests
{
    private static PostgreSqlContainer? _postgres;
    private static ApiTestFactory _factory = null!;
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static async Task InitAsync(TestContext _)
    {
        try
        {
            _postgres = new PostgreSqlBuilder("postgres:17").Build();
            await _postgres.StartAsync();
        }
        catch (Exception exception)
        {
            Assert.Inconclusive($"Docker/Testcontainers unavailable, skipping API integration tests: {exception.Message}");
        }

        _factory = new ApiTestFactory(_postgres!.GetConnectionString());
        _client = _factory.CreateClient();
    }

    [ClassCleanup]
    public static async Task CleanupAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }
    }

    private static WorkflowDefinitionRequest Sample(string name) => new(
        name,
        [new StepDto("check", "Variable", "age >= 18")],
        [new ParameterDto("canVote", "age >= 18")]);

    [TestMethod]
    public async Task Create_then_get_list_and_versions()
    {
        var name = "onboarding-" + Guid.NewGuid().ToString("N");

        var create = await _client.PostAsJsonAsync("/workflows", Sample(name));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<WorkflowResponse>();
        created!.Version.Should().Be(1);
        created.IsLatest.Should().BeTrue();

        var fetched = await _client.GetFromJsonAsync<WorkflowResponse>($"/workflows/{name}");
        fetched!.Steps.Should().ContainSingle().Which.Name.Should().Be("check");

        var versions = await _client.GetFromJsonAsync<List<WorkflowVersionInfo>>($"/workflows/{name}/versions");
        versions!.Should().ContainSingle().Which.Version.Should().Be(1);

        var list = await _client.GetFromJsonAsync<List<WorkflowSummary>>("/workflows");
        list!.Should().Contain(s => s.Name == name);
    }

    [TestMethod]
    public async Task Publishing_twice_creates_a_new_latest_version()
    {
        var name = "billing-" + Guid.NewGuid().ToString("N");

        await _client.PostAsJsonAsync("/workflows", Sample(name));
        var second = await _client.PutAsJsonAsync($"/workflows/{name}", Sample(name));
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        (await second.Content.ReadFromJsonAsync<WorkflowResponse>())!.Version.Should().Be(2);

        var versions = await _client.GetFromJsonAsync<List<WorkflowVersionInfo>>($"/workflows/{name}/versions");
        versions!.Should().HaveCount(2);
        versions.Single(v => v.Version == 1).IsLatest.Should().BeFalse();
        versions.Single(v => v.Version == 2).IsLatest.Should().BeTrue();
    }

    [TestMethod]
    public async Task Validate_reports_malformed_expressions()
    {
        var bad = new WorkflowDefinitionRequest("bad", [new StepDto("s", "Variable", "age >= ")]);

        var response = await _client.PostAsJsonAsync("/workflows/validate", bad);
        var result = await response.Content.ReadFromJsonAsync<ValidationResponse>();

        result!.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [TestMethod]
    public async Task Execute_returns_202_and_creates_a_queryable_execution()
    {
        var name = "exec-" + Guid.NewGuid().ToString("N");
        await _client.PostAsJsonAsync("/workflows", Sample(name));

        var execute = await _client.PostAsJsonAsync($"/workflows/{name}/execute",
            new ExecuteRequest(new Dictionary<string, string> { ["age"] = "20" }));
        execute.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var accepted = await execute.Content.ReadFromJsonAsync<ExecuteResponse>();
        accepted!.ExecutionId.Should().NotBeEmpty();

        // The inline projection builds the status from the appended WorkflowExecutionRequested.
        var status = await _client.GetFromJsonAsync<ExecutionStatusResponse>($"/executions/{accepted.ExecutionId}");
        status!.WorkflowName.Should().Be(name);
        status.Status.Should().Be("Requested");

        var timeline = await _client.GetFromJsonAsync<List<ExecutionEventRecord>>($"/executions/{accepted.ExecutionId}/events");
        timeline!.Should().ContainSingle();
        timeline[0].Type.Should().NotBeNullOrEmpty();

        var byWorkflow = await _client.GetFromJsonAsync<List<ExecutionStatusResponse>>($"/executions?workflow={name}");
        byWorkflow!.Should().ContainSingle().Which.ExecutionId.Should().Be(accepted.ExecutionId);
    }

    [TestMethod]
    public async Task Execute_unknown_workflow_returns_404()
    {
        var response = await _client.PostAsJsonAsync("/workflows/does-not-exist/execute", new ExecuteRequest());
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task Test_run_executes_synchronously_and_returns_output()
    {
        var name = "test-" + Guid.NewGuid().ToString("N");
        var request = new WorkflowDefinitionRequest(
            name,
            [
                new StepDto("set canVote", "SetVariable", null, new Dictionary<string, object>
                {
                    ["Variables"] = new Dictionary<string, string> { ["canVote"] = "age >= 18" }
                })
            ],
            [new ParameterDto("canVote", "canVote")]);
        await _client.PostAsJsonAsync("/workflows", request);

        var run = await _client.PostAsJsonAsync($"/workflows/{name}/test",
            new ExecuteRequest(new Dictionary<string, string> { ["age"] = "20" }));

        run.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await run.Content.ReadFromJsonAsync<TestRunResponse>();
        result!.Success.Should().BeTrue();
        result.Output["canVote"].Should().Be("True");
    }

    [TestMethod]
    public async Task Test_run_executes_control_flow_branch_and_outputs_all()
    {
        var name = "cf-" + Guid.NewGuid().ToString("N");

        // Mirrors the editor's control-flow workflow: init sets canWalk=true/success=false, then a
        // control-flow branch sets success=true when canWalk == true. Output-all captures the context.
        var request = new WorkflowDefinitionRequest(
            name,
            [
                new StepDto("init", "SetVariable", null, new Dictionary<string, object>
                {
                    ["Variables"] = new Dictionary<string, string> { ["canWalk"] = "true", ["success"] = "false" }
                }),
                new StepDto("check", "ControlFlow", null, new Dictionary<string, object>
                {
                    ["Conditions"] = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["Expression"] = "canWalk == true",
                            ["Steps"] = new object[]
                            {
                                new Dictionary<string, object>
                                {
                                    ["name"] = "assign",
                                    ["actionType"] = "SetVariable",
                                    ["properties"] = new Dictionary<string, object>
                                    {
                                        ["Variables"] = new Dictionary<string, string> { ["success"] = "true" }
                                    }
                                }
                            }
                        }
                    }
                })
            ],
            OutputParameters: null,
            Metadata: new Dictionary<string, string> { ["outputAll"] = "true" });
        await _client.PostAsJsonAsync("/workflows", request);

        var run = await _client.PostAsJsonAsync($"/workflows/{name}/test", new ExecuteRequest());

        run.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await run.Content.ReadFromJsonAsync<TestRunResponse>();
        result!.Success.Should().BeTrue();
        result.Output["success"].Should().Be("True");
    }
}
