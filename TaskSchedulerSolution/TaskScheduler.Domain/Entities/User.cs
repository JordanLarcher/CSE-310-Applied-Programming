using System;
using System.Collections.Generic;

namespace TaskScheduler.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsEmailVerified { get; set; }
        public string? GoogleId { get; set; }
        public string? EmailVerificationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry  { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        
        
        // Navigation Properties
        public virtual ICollection<Task> Tasks { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public virtual NotificationSettings NotificationSettings { get; set; }

        public User()
        {
            Tasks = new HashSet<Task>();
            Categories = new HashSet<Category>();
            CreatedAt = DateTime.UtcNow;
        }
    }
}

