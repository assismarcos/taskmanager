using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Time.Testing;
using TaskManager.Api.IntegrationTests.Fixtures;
using TaskManager.Application.Auth;

namespace TaskManager.Api.IntegrationTests;

[Collection("Postgres")]
[Trait("Category", "Integration")]
public abstract class ApiTestBase(PostgresFixture database) : IAsyncLifetime
{
    protected const string Password = "secret123";

    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    // JwtBearer validates lifetime with the real clock, so the fake one starts at the real time.
    protected FakeTimeProvider Time { get; } = new(DateTimeOffset.UtcNow);

    protected CustomWebApplicationFactory Factory { get; private set; } = null!;

    protected HttpClient Client { get; private set; } = null!;

    protected DateOnly Today => DateOnly.FromDateTime(Time.GetUtcNow().UtcDateTime);

    public async ValueTask InitializeAsync()
    {
        await database.ResetAsync();
        Factory = new CustomWebApplicationFactory(database.ConnectionString, Time);
        Client = Factory.CreateClient();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }

    protected async Task<HttpClient> AuthenticateAsync(string email = "ann@mail.com")
    {
        var ct = TestContext.Current.CancellationToken;
        var register = await Client.PostAsJsonAsync("/api/auth/register", new { name = "Ann Lee", email, password = Password }, ct);
        register.EnsureSuccessStatusCode();

        var login = await Client.PostAsJsonAsync("/api/auth/login", new { email, password = Password }, ct);
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<LoginResponse>(Json, ct);

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return client;
    }

    protected static object TaskBody(string title, DateOnly dueDate, string status = "Todo")
        => new { title, description = (string?)null, dueDate, status };
}
