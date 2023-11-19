using ActionFlow.Helpers;
using Newtonsoft.Json;
using NSubstitute;

namespace ActionFlow.Tests.Helpers
{
    [TestClass]
    public class ApiClientTests
    {
        [TestMethod]
        public async Task When_calling_get_it_should_return_result()
        {
            //Arrange
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient().Returns(new HttpClient());
            var url = "http://httpbin.org/get";

            var sut = new ApiClient(httpClientFactory);

            //Act
            var result = await sut.CallGet(url);

            //Assert
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsNotNull(result.Body);
        }

        [TestMethod]
        public async Task When_calling_post_it_should_return_result()
        {
            //Arrange
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient().Returns(new HttpClient());

            var url = "http://httpbin.org/post";
            var data = "{ data: true }";
            var headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" }
            };

            var content = JsonConvert.SerializeObject(data);

            var sut = new ApiClient(httpClientFactory);

            //Act
            var result = await sut.CallPost(url, content, headers);

            //Assert
            Assert.AreEqual(200, result.StatusCode);
        }
    }
}
