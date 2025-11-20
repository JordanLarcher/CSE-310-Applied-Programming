using System;
using TaskScheduler.Domain.Enums;
using TaskStatus = TaskScheduler.Domain.Enums.TaskStatus;

namespace TaskScheduler.Application.DTOs.Tasks
{
    public class UpdateTaskDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskPriority? Priority { get; set; }
        public TaskStatus? Status { get; set; }
        public int? CategoryId { get; set; }
    }
}
