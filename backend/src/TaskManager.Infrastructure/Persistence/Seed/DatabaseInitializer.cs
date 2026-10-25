using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Tasks;
using TaskManager.Domain.Users;

namespace TaskManager.Infrastructure.Persistence.Seed;

public sealed class DatabaseInitializer(AppDbContext context, IPasswordHasher passwordHasher, TimeProvider timeProvider)
{
    private const string DemoEmail = "demo@taskmanager.local";
    private const string OtherEmail = "other@taskmanager.local";

    public async Task InitializeAsync(CancellationToken ct)
    {
        await context.Database.MigrateAsync(ct);

        if (await context.Users.AnyAsync(u => u.Email == DemoEmail, ct))
        {
            return;
        }

        var now = timeProvider.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var seededAt = now.AddDays(-30);

        // Only the documented demo credentials (DD-04 section 4).
        var demo = User.Create("Demo User", DemoEmail, passwordHasher.Hash("Demo@12345"), seededAt);
        var other = User.Create("Other User", OtherEmail, passwordHasher.Hash("Other@12345"), seededAt);
        context.Users.AddRange(demo, other);

        context.Tasks.AddRange(
            BuildTask(demo, "Prepare sprint planning", today.AddDays(2), TaskItemStatus.Todo, seededAt),
            BuildTask(demo, "Review pull requests", today.AddDays(5), TaskItemStatus.Todo, seededAt),
            BuildTask(demo, "Submit expense report", today.AddDays(-3), TaskItemStatus.Todo, seededAt),
            BuildTask(demo, "Plan the product roadmap", today.AddDays(180), TaskItemStatus.Todo, seededAt),
            BuildTask(demo, "Write API documentation", today.AddDays(4), TaskItemStatus.InProgress, seededAt),
            BuildTask(demo, "Fix login redirect bug", today.AddDays(-2), TaskItemStatus.InProgress, seededAt),
            BuildTask(demo, "Set up CI pipeline", today.AddDays(1), TaskItemStatus.Done, seededAt),
            BuildTask(demo, "Update team onboarding guide", today.AddDays(-5), TaskItemStatus.Done, seededAt),
            BuildTask(other, "Book the team offsite", today.AddDays(10), TaskItemStatus.Todo, seededAt),
            BuildTask(other, "Renew software licenses", today.AddDays(3), TaskItemStatus.InProgress, seededAt));

        await context.SaveChangesAsync(ct);
    }

    // Created 30 days back so past due dates respect the domain invariant (DD-04 section 4).
    private static TaskItem BuildTask(User owner, string title, DateOnly dueDate, TaskItemStatus status, DateTimeOffset seededAt) =>
        TaskItem.Create(owner.Id, title, null, dueDate, status, seededAt);
}
