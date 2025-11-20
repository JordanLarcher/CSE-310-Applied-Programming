using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskScheduler.Domain.Entities;
using TaskScheduler.Domain.Enums;
using TaskScheduler.Domain.Interfaces;
using TaskScheduler.Infrastructure.Data;

namespace TaskScheduler.Infrastructure.Repositories
{
    public class TaskRepository : Repository<Domain.Entities.Task>, ITaskRepository
    {
        public TaskRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Domain.Entities.Task>> GetTasksByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Reminders)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Task>> GetTasksByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Reminders)
                .Where(t => t.UserId == userId && t.DueDate >= startDate && t.DueDate <= endDate)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Task>> GetOverdueTasksAsync(int userId)
        {
            return await _dbSet
                .Include(t => t.Category)
                .Where(t => t.UserId == userId &&
                           t.IsOverdue &&
                           t.Status != Domain.Enums.TaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Task>> GetTasksByStatusAsync(int userId, Domain.Enums.TaskStatus status)
        {
            return await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Reminders)
                .Where(t => t.UserId == userId && t.Status == status)
                .OrderByDescending(t => t.UpdatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Task>> GetTasksToCheckForOverdueAsync()
        {
            return await _dbSet
                .Where(t => t.DueDate < DateTime.UtcNow &&
                           !t.IsOverdue &&
                           t.Status != Domain.Enums.TaskStatus.Completed)
                .ToListAsync();
        }

        public async Task<Dictionary<TaskPriority, int>> GetTaskStatisticsAsync(int userId)
        {
            var tasks = await _dbSet
                .Where(t => t.UserId == userId)
                .ToListAsync();

            return tasks
                .GroupBy(t => t.Priority)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<IEnumerable<Domain.Entities.Task>> SearchTasksAsync(int userId, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetTasksByUserIdAsync(userId);

            return await _dbSet
                .Include(t => t.Category)
                .Where(t => t.UserId == userId &&
                           (t.Title.Contains(searchTerm) ||
                            t.Description.Contains(searchTerm)))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Task>> GetTasksByCategoryAsync(int userId, int categoryId)
        {
            return await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Reminders)
                .Where(t => t.UserId == userId && t.CategoryId == categoryId)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public override async Task<Domain.Entities.Task?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(t => t.Category)
                .Include(t => t.Reminders)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
