using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace SampleApp.Tests;

public class HealthCheckTests
{
    [Fact]
    public async Task HealthEndpoint_ShouldReturnHealthy()
    {
        await using var application = new WebApplicationFactory<Program>();

        using var client = application.CreateClient();

        var response = await client.GetAsync("/health");

        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", content);
    }
}