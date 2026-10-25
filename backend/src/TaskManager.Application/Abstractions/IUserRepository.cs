using TaskManager.Domain.Users;

namespace TaskManager.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct);

    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);

    // Returns false when the unique email index rejects the row.
    Task<bool> TryAddAsync(User user, CancellationToken ct);
}
