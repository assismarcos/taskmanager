using Microsoft.Extensions.Options;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Users;

namespace TaskManager.Infrastructure.Authentication;

public sealed class JwtTokenService : ITokenService
{
    public JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
    }

    public AccessToken CreateToken(User user) => throw new NotImplementedException();
}
