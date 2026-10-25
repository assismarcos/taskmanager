using TaskManager.Domain.Users;

namespace TaskManager.Application.UnitTests.Builders;

public sealed class UserBuilder
{
    private string _name = "Ana Souza";
    private string _email = "ana@example.com";
    private string _passwordHash = "stored-hash";
    private DateTimeOffset _now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    public UserBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public User Build() => User.Create(_name, _email, _passwordHash, _now);
}
