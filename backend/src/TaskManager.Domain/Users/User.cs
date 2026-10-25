using TaskManager.Domain.Common;

namespace TaskManager.Domain.Users;

public sealed class User
{
    private const int NameMaxLength = 100;
    private const int EmailMaxLength = 254;

    // Also used by EF Core to materialize rows.
    private User(Guid id, string name, string email, string passwordHash, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static User Create(string name, string email, string passwordHash, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var cleanName = name?.Trim() ?? string.Empty;
        if (cleanName.Length is 0 or > NameMaxLength)
        {
            throw new DomainException($"Name is required and must be at most {NameMaxLength} characters.");
        }

        var cleanEmail = email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (cleanEmail.Length is 0 or > EmailMaxLength)
        {
            throw new DomainException($"Email is required and must be at most {EmailMaxLength} characters.");
        }

        return new User(Guid.CreateVersion7(), cleanName, cleanEmail, passwordHash, now);
    }
}
