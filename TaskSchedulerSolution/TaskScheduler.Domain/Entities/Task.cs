using System;
using System.Collections.Generic;
using TaskScheduler.Domain.Enums;
using TaskStatus = TaskScheduler.Domain.Enums.TaskStatus;

namespace TaskScheduler.Domain.Entities
{
    public class Task
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public int? CategoryId { get; set; }
        public bool IsOverdue { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


        // Navigation Properties
        public virtual User User { get; set; }
        public virtual Category Category { get; set; }
        public ICollection<Reminder> Reminders { get; set; }

        public Task()
        {
            Reminders = new HashSet<Reminder>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Status = TaskStatus.Pending;
        }
    }
}