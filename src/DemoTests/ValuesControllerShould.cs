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

            // Act
            var response = await _client.PutAsync("/api/values/blob", new StringContent(@"{ ""shallow"": [{}] }", Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var actualJson = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(NormalizeJson(expectedJson), NormalizeJson(actualJson));
        }


        [Fact]
        public async Task PostBindPostValuesBlob()
        {
            const string expectedJson = @"{
  ""item1"": {
    ""shallow"": [
      {
        ""value"": ""CustomBindingDemo.Controllers.ValuesController+Shallow""
      }
    ],
    ""value"": ""CustomBindingDemo.Controllers.ValuesController+Deep""
  },
  ""item2"": null
}";

            // Act
            var response = await _client.PostAsync("/api/values/blob", new StringContent(@"{ ""shallow"": [{}] }", Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var actualJson = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(NormalizeJson(expectedJson), NormalizeJson(actualJson));
        }

        private static string NormalizeJson(string json)
        {
            return JsonConvert.DeserializeObject<JObject>(json).ToString();
        }
    }
}
