using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskManager.Api.IntegrationTests.Fixtures;
using TaskManager.Application.Auth;

namespace TaskManager.Api.IntegrationTests;

public sealed class AuthApiTests(PostgresFixture database) : ApiTestBase(database)
{
    [Fact]
    public async Task RegisterThenLogin_ReturnsToken()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var register = new { name = "Ann Lee", email = "Ann@Mail.com", password = Password };

        // Act
        var registered = await Client.PostAsJsonAsync("/api/auth/register", register, ct);
        var login = await Client.PostAsJsonAsync("/api/auth/login", new { email = "ann@mail.com", password = Password }, ct);

        // Assert
        registered.StatusCode.ShouldBe(HttpStatusCode.Created); // AC-1
        registered.Headers.Location!.AbsolutePath.ShouldBe("/api/auth/me");
        login.StatusCode.ShouldBe(HttpStatusCode.OK); // AC-2
        var body = await login.Content.ReadFromJsonAsync<LoginResponse>(Json, ct);
        body!.AccessToken.ShouldNotBeNullOrWhiteSpace();
        body.User.Email.ShouldBe("ann@mail.com");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var register = new { name = "Ann Lee", email = "ann@mail.com", password = Password };
        await Client.PostAsJsonAsync("/api/auth/register", register, ct);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", register, ct);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict); // AC-1
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401WithGenericMessage()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await Client.PostAsJsonAsync("/api/auth/register", new { name = "Ann Lee", email = "ann@mail.com", password = Password }, ct);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", new { email = "ann@mail.com", password = "wrong-pass1" }, ct);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized); // AC-2
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        problem.GetProperty("detail").GetString().ShouldBe("Invalid email or password");
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        // Act
        var response = await Client.GetAsync("/api/auth/me", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized); // AC-9
    }

    [Fact]
    public async Task Me_WithToken_ReturnsUser()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var client = await AuthenticateAsync();

        // Act
        var response = await client.GetAsync("/api/auth/me", ct);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>(Json, ct);
        user!.Email.ShouldBe("ann@mail.com");
        user.Name.ShouldBe("Ann Lee");
    }

    [Fact]
    public async Task Status_WithoutToken_Returns200()
    {
        // Act
        var response = await Client.GetAsync("/api/auth/status", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
