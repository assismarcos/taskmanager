using Microsoft.AspNetCore.Identity;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Users;

namespace TaskManager.Infrastructure.Authentication;

public sealed class PasswordHasher : IPasswordHasher
{
    // PasswordHasher<TUser> never reads the user, so null is safe.
    private readonly PasswordHasher<User> _inner = new();

    public string Hash(string password) => _inner.HashPassword(null!, password);

    public bool Verify(string passwordHash, string password) =>
        _inner.VerifyHashedPassword(null!, passwordHash, password)
            is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}
