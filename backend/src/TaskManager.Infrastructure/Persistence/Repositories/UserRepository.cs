using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Users;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct) =>
        context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<bool> TryAddAsync(User user, CancellationToken ct)
    {
        context.Users.Add(user);
        try
        {
            await context.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            context.Entry(user).State = EntityState.Detached;
            return false;
        }
    }
}
