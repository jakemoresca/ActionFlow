using ActionFlow.Helpers;
using Newtonsoft.Json;

namespace ActionFlow.Tests.Helpers
{
    [TestClass]
    public class ApiClientTests
    {
        [TestMethod]
        public async Task When_calling_get_it_should_return_result()
        {
            //Arrange
            var url = "http://httpbin.org/get";

            //Act
            var result = await ApiClient.CallGet(url);

            //Assert
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsNotNull(result.Body);
        }

        [TestMethod]
        public async Task When_calling_post_it_should_return_result()
        {
            //Arrange
            var url = "http://httpbin.org/post";
            var data = "{ data: true }";
            var headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" }
            };

            var content = JsonConvert.SerializeObject(data);
            var headerJson = JsonConvert.SerializeObject(headers);

            //Act
            var result = await ApiClient.CallPost(url, content, headerJson);

            //Assert
            Assert.AreEqual(200, result.StatusCode);
        }
    }
}
