using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using TaskManager.Domain.Users;
using TaskManager.Infrastructure.Authentication;

namespace TaskManager.Infrastructure.IntegrationTests.Authentication;

public sealed class JwtTokenServiceTests
{
    private static readonly DateTimeOffset _now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    private readonly FakeTimeProvider _time = new(_now);
    private readonly JwtOptions _options = new()
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        Key = "test-signing-key-with-at-least-32-chars",
        ExpirationMinutes = 45,
    };

    private JwtTokenService CreateService() => new(Options.Create(_options), _time);

    [Fact]
    public void CreateToken_ContainsSubEmailAndNameClaims() // AC-2
    {
        // Arrange
        var user = User.Create("Ana Silva", "ana@example.com", "hash", _now);

        // Act
        var accessToken = CreateService().CreateToken(user);

        // Assert
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(accessToken.Token);
        jwt.GetClaim("sub").Value.ShouldBe(user.Id.ToString());
        jwt.GetClaim("email").Value.ShouldBe("ana@example.com");
        jwt.GetClaim("name").Value.ShouldBe("Ana Silva");
        jwt.Issuer.ShouldBe("test-issuer");
        jwt.Audiences.ShouldContain("test-audience");
    }

    [Fact]
    public void CreateToken_ExpiresAfterConfiguredMinutes() // AC-2
    {
        // Arrange
        var user = User.Create("Ana Silva", "ana@example.com", "hash", _now);

        // Act
        var accessToken = CreateService().CreateToken(user);

        // Assert
        var expected = _now.AddMinutes(45);
        accessToken.ExpiresAt.ShouldBe(expected);
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(accessToken.Token);
        new DateTimeOffset(jwt.ValidTo).ShouldBe(expected);
    }
}
