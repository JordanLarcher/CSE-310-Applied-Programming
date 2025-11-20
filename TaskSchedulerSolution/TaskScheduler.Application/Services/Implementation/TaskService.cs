using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TaskScheduler.Application.DTOs.Tasks;
using TaskScheduler.Application.Services.Interfaces;
using TaskScheduler.Domain.Entities;
using TaskScheduler.Domain.Enums;
using TaskScheduler.Domain.Interfaces;
using TaskStatus = TaskScheduler.Domain.Enums.TaskStatus;

namespace TaskScheduler.Application.Services.Implementation
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public TaskService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, int userId)
        {
            var task = new Domain.Entities.Task
            {
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description ?? string.Empty,
                DueDate = dto.DueDate,
                Priority = dto.Priority,
                Status = TaskStatus.Pending,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Tasks.AddAsync(task);

            // Create reminders if specified
            if (dto.ReminderTimes?.Any() == true)
            {
                foreach (var reminderTime in dto.ReminderTimes)
                {
                    var reminder = new Reminder
                    {
                        TaskId = task.Id,
                        ReminderTime = reminderTime,
                        IsSent = false,
                        ReminderType = ReminderType.DeadLine,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Reminders.AddAsync(reminder);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // Send confirmation email
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user.NotificationSettings?.TaskUpdates == true)
            {
                await _emailService.SendTaskCreatedEmailAsync(user.Email, task);
            }

            return MapToDto(task);
        }

        public async Task<TaskDto> UpdateTaskAsync(int taskId, UpdateTaskDto dto, int userId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);

            if (task == null || task.UserId != userId)
                throw new UnauthorizedAccessException("Task not found or access denied");

            task.Title = dto.Title ?? task.Title;
            task.Description = dto.Description ?? task.Description;
            task.DueDate = dto.DueDate ?? task.DueDate;
            task.Priority = dto.Priority ?? task.Priority;
            task.Status = dto.Status ?? task.Status;
            task.CategoryId = dto.CategoryId ?? task.CategoryId;
            task.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == TaskStatus.Completed && task.CompletedAt == null)
            {
                task.CompletedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();

            // Send update notification
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user.NotificationSettings?.TaskUpdates == true)
            {
                await _emailService.SendTaskUpdatedEmailAsync(user.Email, task);
            }

            return MapToDto(task);
        }

        public async Task<bool> DeleteTaskAsync(int taskId, int userId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);

            if (task == null || task.UserId != userId)
                return false;

            await _unitOfWork.Tasks.DeleteAsync(taskId);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<TaskDto> GetTaskByIdAsync(int taskId, int userId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);

            if (task == null || task.UserId != userId)
                throw new UnauthorizedAccessException("Task not found or access denied");

            return MapToDto(task);
        }

        public async Task<IEnumerable<TaskDto>> GetUserTasksAsync(int userId, TaskFilter filter)
        {
            IEnumerable<Domain.Entities.Task> tasks;

            if (filter.Status.HasValue)
            {
                tasks = await _unitOfWork.Tasks.GetTasksByStatusAsync(userId, filter.Status.Value);
            }
            else
            {
                tasks = await _unitOfWork.Tasks.GetTasksByUserIdAsync(userId);
            }

            if (filter.Priority.HasValue)
            {
                tasks = tasks.Where(t => t.Priority == filter.Priority.Value);
            }

            if (filter.CategoryId.HasValue)
            {
                tasks = tasks.Where(t => t.CategoryId == filter.CategoryId.Value);
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                tasks = tasks.Where(t =>
                    t.Title.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            return tasks.Select(MapToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetWeeklyTasksAsync(int userId, DateTime startDate)
        {
            var endDate = startDate.AddDays(7);
            var tasks = await _unitOfWork.Tasks.GetTasksByDateRangeAsync(userId, startDate, endDate);
            return tasks.Select(MapToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetMonthlyTasksAsync(int userId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);
            var tasks = await _unitOfWork.Tasks.GetTasksByDateRangeAsync(userId, startDate, endDate);

            return tasks.Select(MapToDto);
        }

        public async Task<bool> MarkTaskAsCompletedAsync(int taskId, int userId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);

            if (task == null || task.UserId != userId)
                return false;

            task.Status = TaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkTaskAsOverdueAsync(int taskId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);

            if (task == null || task.Status == TaskStatus.Completed)
                return false;

            if (task.DueDate < DateTime.UtcNow && !task.IsOverdue)
            {
                task.IsOverdue = true;
                task.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                // Send overdue notification
                var user = await _unitOfWork.Users.GetByIdAsync(task.UserId);
                if (user.NotificationSettings?.OverdueAlerts == true)
                {
                    await _emailService.SendOverdueTaskEmailAsync(user.Email, task);
                }

                return true;
            }

            return false;
        }

        public async Task<IEnumerable<TaskDto>> GetOverdueTasksAsync(int userId)
        {
            var tasks = await _unitOfWork.Tasks.GetOverdueTasksAsync(userId);
            return tasks.Select(MapToDto);
        }

        public async Task<TaskStatisticsDto> GetTaskStatisticsAsync(int userId)
        {
            var allTasks = await _unitOfWork.Tasks.GetTasksByUserIdAsync(userId);

            return new TaskStatisticsDto
            {
                TotalTasks = allTasks.Count(),
                CompletedTasks = allTasks.Count(t => t.Status == TaskStatus.Completed),
                PendingTasks = allTasks.Count(t => t.Status == TaskStatus.Pending),
                InProgressTasks = allTasks.Count(t => t.Status == TaskStatus.InProgress),
                OverdueTasks = allTasks.Count(t => t.IsOverdue && t.Status != TaskStatus.Completed),
                TasksByPriority = await _unitOfWork.Tasks.GetTaskStatisticsAsync(userId)
            };
        }

        private TaskDto MapToDto(Domain.Entities.Task task)
        {
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                Status = task.Status,
                CategoryId = task.CategoryId,
                CategoryName = task.Category?.Name ?? string.Empty,
                IsOverdue = task.IsOverdue,
                CompletedAt = task.CompletedAt,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
    }
}