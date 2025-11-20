using System;
using TaskScheduler.Domain.Enums;
using TaskStatus = TaskScheduler.Domain.Enums.TaskStatus;

namespace TaskScheduler.Application.DTOs.Tasks
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsOverdue { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
