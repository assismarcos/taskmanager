namespace TaskManager.Domain.Users;

public sealed class User
{
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

    public static User Create(string name, string email, string passwordHash, DateTimeOffset now) =>
        throw new NotImplementedException();
}
