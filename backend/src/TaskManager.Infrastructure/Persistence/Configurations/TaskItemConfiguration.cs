using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Tasks;
using TaskManager.Domain.Users;

namespace TaskManager.Infrastructure.Persistence.Configurations;

internal sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(task => task.Id);
        builder.Property(task => task.Id).ValueGeneratedNever();

        builder.Property(task => task.Title).HasMaxLength(100).IsRequired();
        builder.Property(task => task.Description).HasMaxLength(1000);
        builder.Property(task => task.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(task => task.DueDate).IsRequired();
        builder.Property(task => task.CreatedAt).IsRequired();
        builder.Property(task => task.UpdatedAt).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(task => task.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(task => new { task.UserId, task.Status, task.DueDate })
            .HasDatabaseName("ix_tasks_user_id_status_due_date");
    }
}
