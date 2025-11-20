using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskScheduler.Application.DTOs.Tasks;

namespace TaskScheduler.Application.Services.Interfaces
{
    public interface ITaskService
    {
        Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, int userId);
        Task<TaskDto> UpdateTaskAsync(int taskId, UpdateTaskDto dto, int userId);
        Task<bool> DeleteTaskAsync(int taskId, int userId);
        Task<TaskDto> GetTaskByIdAsync(int taskId, int userId);
        Task<IEnumerable<TaskDto>> GetUserTasksAsync(int userId, TaskFilter filter);
        Task<IEnumerable<TaskDto>> GetWeeklyTasksAsync(int userId, DateTime startDate);
        Task<IEnumerable<TaskDto>> GetMonthlyTasksAsync(int userId, int year, int month);
        Task<bool> MarkTaskAsCompletedAsync(int taskId, int userId);
        Task<bool> MarkTaskAsOverdueAsync(int taskId);
        Task<IEnumerable<TaskDto>> GetOverdueTasksAsync(int userId);
        Task<TaskStatisticsDto> GetTaskStatisticsAsync(int userId);
    }
}
