using TaskManager.Application.Abstractions;

namespace TaskManager.Infrastructure.Persistence.Seed;

public sealed class DatabaseInitializer
{
    public DatabaseInitializer(AppDbContext context, IPasswordHasher passwordHasher, TimeProvider timeProvider)
    {
    }

    public Task InitializeAsync(CancellationToken ct) => throw new NotImplementedException();
}
