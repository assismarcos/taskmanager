using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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

    [Theory]
    [InlineData("auth-v1")]
    [InlineData("tasks-v1")]
    public async Task OpenApi_ForEachDocument_DeclaresTheBearerScheme(string document)
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;

        // Act
        var json = await Client.GetFromJsonAsync<JsonElement>($"/openapi/{document}.json", ct);

        // Assert
        var scheme = json.GetProperty("components").GetProperty("securitySchemes").GetProperty("bearer");
        scheme.GetProperty("type").GetString().ShouldBe("http");
        scheme.GetProperty("scheme").GetString().ShouldBe("bearer");
        json.GetProperty("security").GetArrayLength().ShouldBe(1);
    }

    [Fact]
    public async Task OpenApi_TasksDocument_DescribesStatusAsAStringEnum()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;

        // Act
        var json = await Client.GetFromJsonAsync<JsonElement>("/openapi/tasks-v1.json", ct);

        // Assert
        var status = json.GetProperty("components").GetProperty("schemas").GetProperty("TaskItemStatus");
        status.GetProperty("enum").EnumerateArray().Select(e => e.GetString())
            .ShouldBe(["Todo", "InProgress", "Done"]);
    }

    [Fact]
    public async Task Login_FromDifferentForwardedClients_UsesSeparateRateLimits()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        using var factory = Factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("RateLimiting:LoginPermitLimit", "1");
            builder.ConfigureServices(services => services.AddTransient<IStartupFilter, ProxyAddressStartupFilter>());
        });
        var client = factory.CreateClient();

        async Task<HttpStatusCode> LoginFromAsync(string clientIp)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
            {
                Content = JsonContent.Create(new { email = "ann@mail.com", password = Password }),
            };
            request.Headers.Add("X-Forwarded-For", clientIp);
            return (await client.SendAsync(request, ct)).StatusCode;
        }

        // Act
        var firstFromA = await LoginFromAsync("203.0.113.1");
        var secondFromA = await LoginFromAsync("203.0.113.1");
        var firstFromB = await LoginFromAsync("203.0.113.2");

        // Assert
        firstFromA.ShouldNotBe(HttpStatusCode.TooManyRequests);
        secondFromA.ShouldBe(HttpStatusCode.TooManyRequests);
        firstFromB.ShouldNotBe(HttpStatusCode.TooManyRequests);
    }

    // The test server has no socket, so it plays the compose nginx container (a private address).
    private sealed class ProxyAddressStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use((context, nextMiddleware) =>
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse("172.18.0.2");
                return nextMiddleware(context);
            });
            next(app);
        };
    }
}
