using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TaskScheduler.Domain.Enums;

namespace TaskScheduler.Application.DTOs.Tasks
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public required string Title { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        public TaskPriority Priority { get; set; }

        public int? CategoryId { get; set; }

        public List<DateTime> ReminderTimes { get; set; } = new();
    }
}