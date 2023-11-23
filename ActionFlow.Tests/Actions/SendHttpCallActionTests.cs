using ActionFlow.Actions;
using ActionFlow.Domain.Actions;
using ActionFlow.Engine.Factories;
using ActionFlow.Engine;
using ActionFlow.Helpers;
using NSubstitute;

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
            var data = "{ data: true }";

            executionContext.AddOrUpdateActionProperty(SendHttpCallAction.UrlKey, "http://httpbin.org/post");
            executionContext.AddOrUpdateActionProperty(SendHttpCallAction.MethodKey, "POST");
            executionContext.AddOrUpdateActionProperty(SendHttpCallAction.HeadersKey, headers);
            executionContext.AddOrUpdateActionProperty(SendHttpCallAction.BodyKey, data);
            executionContext.AddOrUpdateActionProperty(SendHttpCallAction.ResultVariableKey, "output");

            sut.SetExecutionContext(executionContext);

            //Act
            await sut.ExecuteAction();

            //Assert
            Assert.IsNotNull(executionContext.EvaluateExpression<ApiCallResult>("output"));

            var output = executionContext.EvaluateExpression<ApiCallResult>("output");
            Assert.AreEqual(200, output.StatusCode);
            Assert.AreEqual(headers["Accept"], output.Body["headers"]!["Accept"]!.GetValue<string>());
        }
    }
}
