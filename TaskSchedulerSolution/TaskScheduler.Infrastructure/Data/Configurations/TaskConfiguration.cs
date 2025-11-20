using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskScheduler.Infrastructure.Data.Configurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<Domain.Entities.Task>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Task> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Description)
                .HasMaxLength(2000);

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(t => t.Priority)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.HasIndex(t => t.UserId);
            builder.HasIndex(t => t.CategoryId);
            builder.HasIndex(t => t.Status);
            builder.HasIndex(t => t.DueDate);

            // Relationships
            builder.HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.Category)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(t => t.Reminders)
                .WithOne(r => r.Task)
                .HasForeignKey(r => r.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
