using Microsoft.Extensions.Logging;
using TaskScheduler.Application.Services.Interfaces;
using TaskScheduler.Domain.Interfaces;

namespace TaskScheduler.Application.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendTaskReminderNotificationAsync(int userId, int taskId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);

            if (user == null || task == null)
            {
                _logger.LogWarning($"User {userId} or Task {taskId} not found");
                return;
            }

            var settings = await _unitOfWork.NotificationSettings.GetByUserIdAsync(userId);
            if (settings?.EmailReminders == true)
            {
                await _emailService.SendTaskReminderEmailAsync(user.Email, task);
                _logger.LogInformation($"Sent reminder notification for task {taskId} to user {userId}");
            }
        }

        public async Task SendOverdueTaskNotificationAsync(int userId, int taskId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);

            if (user == null || task == null)
            {
                _logger.LogWarning($"User {userId} or Task {taskId} not found");
                return;
            }

            var settings = await _unitOfWork.NotificationSettings.GetByUserIdAsync(userId);
            if (settings?.OverdueAlerts == true)
            {
                await _emailService.SendOverdueTaskEmailAsync(user.Email, task);
                _logger.LogInformation($"Sent overdue notification for task {taskId} to user {userId}");
            }
        }

        public async Task SendDailyDigestAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User {userId} not found");
                return;
            }

            var settings = await _unitOfWork.NotificationSettings.GetByUserIdAsync(userId);
            if (settings?.DailyDigest != true)
            {
                return;
            }

            var tasks = await _unitOfWork.Tasks.GetTasksByUserIdAsync(userId);
            var todayTasks = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == DateTime.UtcNow.Date).ToList();

            if (todayTasks.Any())
            {
                _logger.LogInformation($"Sending daily digest to user {userId} with {todayTasks.Count} tasks");
                // Email sending logic would go here
            }
        }

        public async Task SendWeeklySummaryAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User {userId} not found");
                return;
            }

            var settings = await _unitOfWork.NotificationSettings.GetByUserIdAsync(userId);
            if (settings?.WeeklySummary != true)
            {
                return;
            }

            var tasks = await _unitOfWork.Tasks.GetTasksByUserIdAsync(userId);
            var weekStart = DateTime.UtcNow.AddDays(-7);
            var weekTasks = tasks.Where(t => t.CreatedAt >= weekStart).ToList();

            _logger.LogInformation($"Sending weekly summary to user {userId} with {weekTasks.Count} tasks");
            // Email sending logic would go here
        }

        public async Task ProcessPendingNotificationsAsync()
        {
            var now = DateTime.UtcNow;
            var dueReminders = await _unitOfWork.Reminders.GetDueRemindersAsync(now);

            foreach (var reminder in dueReminders)
            {
                try
                {
                    var task = await _unitOfWork.Tasks.GetByIdAsync(reminder.TaskId);
                    if (task != null && task.User != null)
                    {
                        await SendTaskReminderNotificationAsync(task.UserId, task.Id);
                        await _unitOfWork.Reminders.MarkAsSentAsync(reminder.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing reminder {reminder.Id}");
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
