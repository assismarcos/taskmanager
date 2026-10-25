using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Tasks;
using TaskManager.Domain.Users;

namespace TaskManager.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public static DbContextOptionsBuilder ConfigureOptions(DbContextOptionsBuilder builder, string connectionString) =>
        builder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
