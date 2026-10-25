using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Respawn.Graph;
using TaskManager.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace TaskManager.Api.IntegrationTests.Fixtures;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine").Build();
    private Respawner _respawner = null!;

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        await _container.StartAsync(ct);

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        AppDbContext.ConfigureOptions(builder, ConnectionString);
        await using (var context = new AppDbContext(builder.Options))
        {
            await context.Database.MigrateAsync(ct);
        }

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(ct);
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = [new Table("__EFMigrationsHistory")],
        });
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await _respawner.ResetAsync(connection);
    }
}
