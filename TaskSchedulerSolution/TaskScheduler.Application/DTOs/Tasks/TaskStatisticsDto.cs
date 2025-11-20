using System.Collections.Generic;
using TaskScheduler.Domain.Enums;

namespace TaskScheduler.Application.DTOs.Tasks
{
    public class TaskStatisticsDto
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public Dictionary<TaskPriority, int> TasksByPriority { get; set; }
    }
}
