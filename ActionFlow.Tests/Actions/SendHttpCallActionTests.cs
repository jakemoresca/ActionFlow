using ActionFlow.Actions;
using ActionFlow.Domain.Actions;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine;
using ActionFlow.Helpers;
using NSubstitute;
using FluentAssertions;
using System.Text.Json;

namespace ActionFlow.Tests.Actions
{
    [TestClass]
    public class SendHttpCallActionTests : ActionBaseTests
    {
        [TestMethod]
        public async Task When_executing_with_get_it_should_set_result_variable()
        {
            //Arrange
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient().Returns(new HttpClient());
            var sut = new SendHttpCallAction(new ApiClient(httpClientFactory));
            var executionContext = ExecutionContext;

            executionContext.AddOrUpdateActionProperty(SendHttpCallAction.UrlKey, "http://httpbin.org/get");
            executionContext.AddOrUpdateActionProperty(SendHttpCallAction.MethodKey, "GET");
            executionContext.AddOrUpdateActionProperty(SendHttpCallAction.ResultVariableKey, "output");

            sut.SetExecutionContext(executionContext);

            //Act
            await sut.ExecuteAction();

            //Assert
            Assert.IsNotNull(executionContext.EvaluateExpression<ApiCallResult>("output"));

            var output = executionContext.EvaluateExpression<ApiCallResult>("output");
            Assert.AreEqual(200, output.StatusCode);
        }

        [TestMethod]
        public async Task When_executing_with_post_it_should_set_result_variable()
        {
            //Arrange
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient().Returns(new HttpClient());
            var sut = new SendHttpCallAction(new ApiClient(httpClientFactory));
            var executionContext = ExecutionContext;

            var headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" }
            };

            var dataValue = true;
            var data = new Dictionary<string, object>
            {
                { "data", dataValue }
            };
            var dataJson = JsonSerializer.Serialize(data);

            executionContext.AddOrUpdateActionProperty(SendHttpCallAction.UrlKey, "http://httpbin.org/post");
            executionContext.AddOrUpdateActionProperty(SendHttpCallAction.MethodKey, "POST");
            executionContext.AddOrUpdateActionProperty(SendHttpCallAction.HeadersKey, headers);
            executionContext.AddOrUpdateActionProperty(SendHttpCallAction.BodyKey, dataJson);
            executionContext.AddOrUpdateActionProperty(SendHttpCallAction.ResultVariableKey, "output");

            sut.SetExecutionContext(executionContext);

            //Act
            await sut.ExecuteAction();

            //Assert
            Assert.IsNotNull(executionContext.EvaluateExpression<ApiCallResult>("output"));

            var output = executionContext.EvaluateExpression<ApiCallResult>("output");
            Assert.AreEqual(200, output.StatusCode);
            Assert.AreEqual(headers["Accept"], output.Body["headers"]!["Accept"]!.GetValue<string>());

            var jsonBody = output.Body["json"]!.ToString();
            var outputData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBody);
            Assert.AreEqual(dataValue, bool.Parse(outputData!["data"]!.ToString()!));
        }
    }
}
