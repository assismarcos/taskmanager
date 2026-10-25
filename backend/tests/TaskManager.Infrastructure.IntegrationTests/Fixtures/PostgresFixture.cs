using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Respawn.Graph;
using TaskManager.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace TaskManager.Infrastructure.IntegrationTests.Fixtures;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine").Build();
    private Respawner _respawner = null!;

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        await _container.StartAsync(ct);

        await using var context = CreateContext();
        await context.Database.MigrateAsync(ct);

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(ct);
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = [new Table("__EFMigrationsHistory")],
        });
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public AppDbContext CreateContext()
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        AppDbContext.ConfigureOptions(builder, ConnectionString);
        return new AppDbContext(builder.Options);
    }

    public async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await _respawner.ResetAsync(connection);
    }
}
