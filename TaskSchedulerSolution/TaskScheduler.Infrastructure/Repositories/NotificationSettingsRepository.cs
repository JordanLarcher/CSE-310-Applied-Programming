using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskScheduler.Domain.Entities;
using TaskScheduler.Domain.Interfaces;
using TaskScheduler.Infrastructure.Data;

namespace TaskScheduler.Infrastructure.Repositories
{
    public class NotificationSettingsRepository : Repository<NotificationSettings>, INotificationSettingsRepository
    {
        public NotificationSettingsRepository(ApplicationDbContext context) : base(context) { }

        public async Task<NotificationSettings?> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(ns => ns.UserId == userId);
        }
    }
}