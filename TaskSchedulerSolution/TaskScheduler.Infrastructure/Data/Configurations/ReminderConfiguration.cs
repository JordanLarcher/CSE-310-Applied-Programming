using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskScheduler.Domain.Entities;

namespace TaskScheduler.Infrastructure.Data.Configurations
{
    public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
    {
        public void Configure(EntityTypeBuilder<Reminder> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.ReminderTime)
                .IsRequired();

            builder.Property(r => r.ReminderType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(r => r.IsSent)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(r => r.TaskId);
            builder.HasIndex(r => r.ReminderTime);
            builder.HasIndex(r => r.IsSent);

            // Relationships
            builder.HasOne(r => r.Task)
                .WithMany(t => t.Reminders)
                .HasForeignKey(r => r.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
