using System;

namespace TaskScheduler.Domain.Entities
{
    public class NotificationSettings
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool EmailReminders { get; set; }
        public bool OverdueAlerts { get; set; }
        public bool TaskUpdates { get; set; }
        public bool DailyDigest { get; set; }
        public bool WeeklySummary { get; set; }
        public TimeSpan DailyDigestTime  { get; set; }
        public DayOfWeek WeeklySummaryDay { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        
        // Navigation property 
        public virtual User User { get; set; }

        public NotificationSettings()
        {
            EmailReminders = true;
            OverdueAlerts = true;
            TaskUpdates = true;
            DailyDigest = false;
            WeeklySummary = true;
            DailyDigestTime = TimeSpan.FromHours(8);
            WeeklySummaryDay = DayOfWeek.Sunday;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

