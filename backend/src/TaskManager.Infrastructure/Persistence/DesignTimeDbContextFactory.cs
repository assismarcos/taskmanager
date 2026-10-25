using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskManager.Infrastructure.Persistence;

internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    // Local-only fallback without credentials: migrations are generated without opening a connection.
    private const string LocalFallback = "Host=localhost;Database=taskmanager";

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default") ?? LocalFallback;
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        AppDbContext.ConfigureOptions(builder, connectionString);
        return new AppDbContext(builder.Options);
    }
}
