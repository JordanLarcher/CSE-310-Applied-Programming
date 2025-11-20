using System;
using System.Threading.Tasks;

namespace TaskScheduler.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ITaskRepository Tasks { get; }
        IReminderRepository Reminders { get; }
        ICategoryRepository Categories { get; }
        INotificationSettingsRepository NotificationSettings { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollBackAsync();
    }
}