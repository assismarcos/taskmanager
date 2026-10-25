using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Users;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct) => throw new NotImplementedException();

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) => context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<bool> TryAddAsync(User user, CancellationToken ct) => throw new NotImplementedException();
}
