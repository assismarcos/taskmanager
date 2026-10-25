using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
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

    private static User CreateUser() => User.Create("Ana Silva", "ana@example.com", "hash", _now);

    [Fact]
    public void CreateToken_ContainsSubEmailAndNameClaims() // AC-2
    {
        // Arrange
        var user = CreateUser();

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
        var user = CreateUser();

        // Act
        var accessToken = CreateService().CreateToken(user);

        // Assert
        var expected = _now.AddMinutes(45);
        accessToken.ExpiresAt.ShouldBe(expected);
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(accessToken.Token);
        new DateTimeOffset(jwt.ValidTo).ShouldBe(expected);
    }

    [Fact]
    public void CreateToken_UsesTheInjectedClockForIssuedAtAndValidFrom()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var accessToken = CreateService().CreateToken(user);

        // Assert
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(accessToken.Token);
        new DateTimeOffset(jwt.IssuedAt).ShouldBe(_now);
        new DateTimeOffset(jwt.ValidFrom).ShouldBe(_now);
    }

    [Fact]
    public void CreateToken_SignsWithHmacSha256()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var accessToken = CreateService().CreateToken(user);

        // Assert
        new JsonWebTokenHandler().ReadJsonWebToken(accessToken.Token).Alg.ShouldBe(SecurityAlgorithms.HmacSha256);
    }

    [Fact]
    public async Task CreateToken_ProducesTokenThatValidatesWithTheConfiguredKeyIssuerAndAudience()
    {
        // Arrange
        var accessToken = CreateService().CreateToken(CreateUser());
        var parameters = new TokenValidationParameters
        {
            IssuerSigningKey = _options.CreateSigningKey(),
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            ValidateLifetime = false,
        };

        // Act
        var result = await new JsonWebTokenHandler().ValidateTokenAsync(accessToken.Token, parameters);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void CreateToken_WithSubSecondClock_ReturnsExpiryEqualToTheTokenExp()
    {
        // Arrange
        _time.SetUtcNow(_now.AddMilliseconds(789));

        // Act
        var accessToken = CreateService().CreateToken(CreateUser());

        // Assert
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(accessToken.Token);
        accessToken.ExpiresAt.ShouldBe(new DateTimeOffset(jwt.ValidTo));
    }
}
