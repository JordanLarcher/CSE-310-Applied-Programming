namespace TaskScheduler.Domain.Interfaces;

using System.Threading.Tasks;
using TaskScheduler.Domain.Entities;

public interface INotificationSettingsRepository : IRepository<NotificationSettings>
{
    Task<NotificationSettings?> GetByUserIdAsync(int userId);
}