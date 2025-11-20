using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskScheduler.Domain.Entities;

namespace TaskScheduler.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.GoogleId)
                .HasMaxLength(255);

            builder.HasIndex(u => u.GoogleId);

            builder.Property(u => u.EmailVerificationToken)
                .HasMaxLength(500);

            builder.Property(u => u.PasswordResetToken)
                .HasMaxLength(500);

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            // Relationships
            builder.HasMany(u => u.Tasks)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Categories)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.NotificationSettings)
                .WithOne(ns => ns.User)
                .HasForeignKey<NotificationSettings>(ns => ns.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
