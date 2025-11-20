using TaskScheduler.Domain.Enums;
using TaskStatus = TaskScheduler.Domain.Enums.TaskStatus;

namespace TaskScheduler.Domain.Interfaces
{
    public interface ITaskRepository : IRepository<Entities.Task>
    {
        System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetTasksByUserIdAsync(int userId);
        System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetTasksByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
        System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetOverdueTasksAsync(int userId);
        System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetTasksByStatusAsync(int userId, TaskStatus status);
        System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetTasksToCheckForOverdueAsync();
        System.Threading.Tasks.Task<Dictionary<TaskPriority, int>> GetTaskStatisticsAsync(int userId);
        System.Threading.Tasks.Task<IEnumerable<Entities.Task>> SearchTasksAsync(int userId, string searchTerm);
        System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetTasksByCategoryAsync(int userId, int categoryId);
    }
}