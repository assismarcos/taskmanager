using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.IntegrationTests.Fixtures;

namespace TaskManager.Infrastructure.IntegrationTests.Persistence;

[Collection("Postgres")]
[Trait("Category", "Integration")]
public sealed class MigrationTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Migrations_ApplyToEmptyDatabase()
    {
        // Arrange
        await using var context = fixture.CreateContext();

        // Act
        var pending = await context.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken);
        var applied = await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken);

        // Assert
        pending.ShouldBeEmpty();
        applied.ShouldNotBeEmpty();
        (await context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
        (await context.Tasks.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }
}
