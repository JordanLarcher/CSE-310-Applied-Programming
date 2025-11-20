using TaskScheduler.Domain.Enums;
using TaskStatus = TaskScheduler.Domain.Enums.TaskStatus;

namespace TaskScheduler.Application.DTOs.Tasks
{
    public class TaskFilter
    {
        public TaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public int? CategoryId { get; set; }
        public string SearchTerm { get; set; }
    }
}
