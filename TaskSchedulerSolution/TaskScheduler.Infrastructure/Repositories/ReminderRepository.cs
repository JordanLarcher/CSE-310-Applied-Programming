using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskScheduler.Domain.Entities;
using TaskScheduler.Domain.Interfaces;
using TaskScheduler.Infrastructure.Data;

namespace TaskScheduler.Infrastructure.Repositories
{
    public class ReminderRepository : Repository<Reminder>, IReminderRepository
    {
        public ReminderRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Reminder>> GetDueRemindersAsync(DateTime currentTime)
        {
            return await _dbSet
                .Include(r => r.Task)
                .Where(r => !r.IsSent && r.ReminderTime <= currentTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reminder>> GetRemindersByTaskIdAsync(int taskId)
        {
            return await _dbSet
                .Where(r => r.TaskId == taskId)
                .OrderBy(r => r.ReminderTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reminder>> GetUnsentRemindersAsync()
        {
            return await _dbSet
                .Include(r => r.Task)
                .Where(r => !r.IsSent && r.ReminderTime <= DateTime.UtcNow)
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task MarkAsSentAsync(int reminderId)
        {
            var reminder = await GetByIdAsync(reminderId);
            if (reminder != null)
            {
                reminder.IsSent = true;
                reminder.SentAt = DateTime.UtcNow;
            }
        }
    }
}