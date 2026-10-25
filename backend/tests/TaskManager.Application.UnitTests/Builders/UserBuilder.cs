using TaskManager.Domain.Users;

namespace TaskManager.Application.UnitTests.Builders;

public sealed class UserBuilder
{
    private readonly string _name = "Ana Souza";
    private readonly string _email = "ana@example.com";
    private readonly DateTimeOffset _now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);
    private string _passwordHash = "stored-hash";

    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public User Build() => User.Create(_name, _email, _passwordHash, _now);
}
