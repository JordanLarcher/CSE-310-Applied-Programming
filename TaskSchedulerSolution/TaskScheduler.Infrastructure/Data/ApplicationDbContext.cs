using Microsoft.EntityFrameworkCore;
using TaskScheduler.Domain.Entities;

namespace TaskScheduler.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Domain.Entities.Task> Tasks { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            
            // Additional configurations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            });
            
            modelBuilder.Entity<Domain.Entities.Task>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Tasks)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.DueDate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.UserId, e.Status });
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            });
            
            modelBuilder.Entity<Reminder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Task)
                      .WithMany(t => t.Reminders)
                      .HasForeignKey(e => e.TaskId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.ReminderTime, e.IsSent });
            });
            
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Categories)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });
            
            modelBuilder.Entity<NotificationSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithOne(u => u.NotificationSettings)
                      .HasForeignKey<NotificationSettings>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}