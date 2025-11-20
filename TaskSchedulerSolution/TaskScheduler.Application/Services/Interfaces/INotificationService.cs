using System.Threading.Tasks;
using TaskScheduler.Domain.Entities;

namespace TaskScheduler.Application.Services.Interfaces
{
    public interface INotificationService
    {
        System.Threading.Tasks.Task SendTaskReminderNotificationAsync(int userId, int taskId);
        System.Threading.Tasks.Task SendOverdueTaskNotificationAsync(int userId, int taskId);
        System.Threading.Tasks.Task SendDailyDigestAsync(int userId);
        System.Threading.Tasks.Task SendWeeklySummaryAsync(int userId);
        System.Threading.Tasks.Task ProcessPendingNotificationsAsync();
    }
}
