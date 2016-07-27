using CustomBindingDemo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DemoTests
{
    public class ValuesControllerShould
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public ValuesControllerShould()
        {
            // Arrange
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task PostBindPutValuesBlob()
        {
            const string expectedJson = @"{
  ""body"": {
    ""shallow"": [
      {
        ""value"": ""CustomBindingDemo.Controllers.ValuesController+Shallow""
      }
    ],
    ""value"": ""CustomBindingDemo.Controllers.ValuesController+Deep""
  },
  ""value"": ""CustomBindingDemo.Controllers.ValuesController+FullRequest"",
  ""id"": ""blob""
}";
            var expectedParsed = JsonConvert.DeserializeObject<JObject>(expectedJson);

            // Act
            var response = await _client.PutAsync("/api/values/blob", new StringContent(@"{ ""shallow"": [{}] }", Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var actualParsed = JsonConvert.DeserializeObject<JObject>(responseString);

            // Assert
            Assert.Equal(expectedParsed.ToString(), actualParsed.ToString());
        }
    }
}
