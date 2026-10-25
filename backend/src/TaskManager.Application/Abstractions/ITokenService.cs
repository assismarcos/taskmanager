using TaskManager.Domain.Users;

namespace TaskManager.Application.Abstractions;

public interface ITokenService
{
    AccessToken CreateToken(User user);
}
