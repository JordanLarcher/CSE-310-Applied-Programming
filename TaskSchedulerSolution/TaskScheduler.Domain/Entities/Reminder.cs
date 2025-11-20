using System;
using TaskScheduler.Domain.Enums;

namespace TaskScheduler.Domain.Entities
{
    public class Reminder
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime ReminderTime { get; set; }
        public bool IsSent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public ReminderType ReminderType { get; set; }
        
        // Navigation property
        public virtual Task Task { get; set; }

        public Reminder()
        {
            CreatedAt = DateTime.UtcNow;
            IsSent = false;
        }
    }
}