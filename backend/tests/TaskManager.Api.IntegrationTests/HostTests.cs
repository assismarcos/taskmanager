using System.Net;
using TaskManager.Api.IntegrationTests.Fixtures;

namespace TaskManager.Api.IntegrationTests;

public sealed class HostTests(PostgresFixture database) : ApiTestBase(database)
{
    [Fact]
    public async Task Health_WithoutToken_Returns200()
    {
        // Act
        var response = await Client.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("auth-v1")]
    [InlineData("tasks-v1")]
    public async Task OpenApi_ForEachDocument_ReturnsTheDocument(string document)
    {
        // Act
        var response = await Client.GetAsync($"/openapi/{document}.json", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
