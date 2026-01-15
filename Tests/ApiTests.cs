using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace Tests
{
    public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder => { });
        }

        [Fact]
        public async Task GetStations_Returns_Data()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v1/stations");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));
        }
    }
}
