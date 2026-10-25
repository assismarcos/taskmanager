using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Users;

namespace TaskManager.Infrastructure.Authentication;

public sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
{
    private readonly JwtOptions _options = options.Value;
    private readonly JsonWebTokenHandler _handler = new();
    private readonly SigningCredentials _credentials = new(options.Value.CreateSigningKey(), SecurityAlgorithms.HmacSha256);

    public AccessToken CreateToken(User user)
    {
        // JWT times are whole seconds; truncating keeps ExpiresAt equal to the exp claim.
        var now = timeProvider.GetUtcNow();
        now = new DateTimeOffset(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond), now.Offset);
        var expiresAt = now.AddMinutes(_options.ExpirationMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = user.Email,
                [JwtRegisteredClaimNames.Name] = user.Name,
            },
            SigningCredentials = _credentials,
        };

        return new AccessToken(_handler.CreateToken(descriptor), expiresAt);
    }
}
