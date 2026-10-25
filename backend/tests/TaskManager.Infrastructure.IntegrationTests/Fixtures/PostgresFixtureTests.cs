using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Tasks;
using TaskManager.Domain.Users;

namespace TaskManager.Infrastructure.IntegrationTests.Fixtures;

[Collection("Postgres")]
[Trait("Category", "Integration")]
public sealed class PostgresFixtureTests(PostgresFixture fixture) : IAsyncLifetime
{
    private static readonly DateTimeOffset _now = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ResetAsync_RemovesAllRows()
    {
        // Arrange
        await using (var seed = fixture.CreateContext())
        {
            var user = User.Create("Ana", "ana@example.com", "hash", _now);
            seed.Users.Add(user);
            seed.Tasks.Add(TaskItem.Create(user.Id, "Write report", "Quarterly", new DateOnly(2026, 2, 1), TaskItemStatus.Todo, _now));
            await seed.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        await fixture.ResetAsync();

        // Assert
        await using var context = fixture.CreateContext();
        (await context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
        (await context.Tasks.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task ResetAsync_KeepsMigrationHistory()
    {
        // Arrange
        await using var context = fixture.CreateContext();

        // Act
        await fixture.ResetAsync();

        // Assert
        var applied = await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken);
        applied.ShouldBe(context.Database.GetMigrations());
    }
}
