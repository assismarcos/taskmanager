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

        // Only the documented demo credentials (DD-04 section 4).
        var demo = User.Create("Demo User", DemoEmail, passwordHasher.Hash("Demo@12345"), now);
        var other = User.Create("Other User", OtherEmail, passwordHasher.Hash("Other@12345"), now);
        context.Users.AddRange(demo, other);

        context.Tasks.AddRange(
            BuildTask(demo, "Prepare sprint planning", today.AddDays(2), TaskItemStatus.Todo, now),
            BuildTask(demo, "Review pull requests", today.AddDays(5), TaskItemStatus.Todo, now),
            BuildTask(demo, "Submit expense report", today.AddDays(-3), TaskItemStatus.Todo, now),
            BuildTask(demo, "Plan the product roadmap", today.AddDays(180), TaskItemStatus.Todo, now),
            BuildTask(demo, "Write API documentation", today.AddDays(4), TaskItemStatus.InProgress, now),
            BuildTask(demo, "Fix login redirect bug", today.AddDays(-2), TaskItemStatus.InProgress, now),
            BuildTask(demo, "Set up CI pipeline", today.AddDays(1), TaskItemStatus.Done, now),
            BuildTask(demo, "Update team onboarding guide", today.AddDays(-5), TaskItemStatus.Done, now),
            BuildTask(other, "Book the team offsite", today.AddDays(10), TaskItemStatus.Todo, now),
            BuildTask(other, "Renew software licenses", today.AddDays(3), TaskItemStatus.InProgress, now));

        await context.SaveChangesAsync(ct);
    }

    private static TaskItem BuildTask(User owner, string title, DateOnly dueDate, TaskItemStatus status, DateTimeOffset now)
    {
        // Created in the past so a past due date respects the domain invariant (DD-04 section 4).
        var createdAt = dueDate < DateOnly.FromDateTime(now.UtcDateTime) ? now.AddDays(-14) : now;
        var task = TaskItem.Create(owner.Id, title, null, dueDate, TaskItemStatus.Todo, createdAt);
        task.ChangeStatus(status, createdAt);
        return task;
    }
}
