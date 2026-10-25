using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.IntegrationTests.Fixtures;

namespace TaskManager.Infrastructure.IntegrationTests.Persistence;

[Collection("Postgres")]
[Trait("Category", "Integration")]
public sealed class MigrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Migrations_ApplyToEmptyDatabase()
    {
        // Arrange
        await using var context = fixture.CreateContext();

        // Act
        var applied = await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken);
        var indexes = await context.Database
            .SqlQuery<string>($"SELECT indexname AS \"Value\" FROM pg_indexes WHERE tablename = 'tasks'")
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        applied.ShouldBe(context.Database.GetMigrations());
        context.Database.HasPendingModelChanges().ShouldBeFalse();
        indexes.ShouldContain("ix_tasks_user_id_status_due_date");
    }
}
