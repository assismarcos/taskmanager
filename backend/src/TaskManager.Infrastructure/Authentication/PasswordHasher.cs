using TaskManager.Application.Abstractions;

namespace TaskManager.Infrastructure.Authentication;

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => throw new NotImplementedException();

    public bool Verify(string passwordHash, string password) => throw new NotImplementedException();
}
