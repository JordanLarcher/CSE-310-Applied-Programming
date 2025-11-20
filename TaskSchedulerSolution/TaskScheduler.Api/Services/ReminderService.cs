using Microsoft.Extensions.DependencyInjection;
using TaskScheduler.Domain.Interfaces;
using TaskScheduler.Application.Services.Interfaces;

namespace TaskScheduler.Api.Services
{
    public class ReminderService : BackgroundService
    {
        private readonly ILogger<ReminderService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ReminderService(ILogger<ReminderService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reminder Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Reminder Service is checking for reminders...");
                await CheckForReminders();

                // Wait for 1 minute before checking again
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("Reminder Service is stopping.");
        }

        private async Task CheckForReminders()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            try
            {
                // Get all due reminders that haven't been sent yet
                var dueReminders = await unitOfWork.Reminders.GetDueRemindersAsync(DateTime.UtcNow);

                foreach (var reminder in dueReminders)
                {
                    try
                    {
                        // Load the task with user info
                        var task = await unitOfWork.Tasks.GetByIdAsync(reminder.TaskId);

                        if (task == null || task.User == null)
                        {
                            _logger.LogWarning($"Task {reminder.TaskId} not found for reminder {reminder.Id}");
                            continue;
                        }

                        // Send reminder email
                        await emailService.SendTaskReminderEmailAsync(
                            task.User.Email,
                            task
                        );

                        // Mark reminder as sent
                        await unitOfWork.Reminders.MarkAsSentAsync(reminder.Id);

                        _logger.LogInformation($"Sent reminder for task '{task.Title}' to {task.User.Email}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sending reminder {reminder.Id}: {ex.Message}");
                    }
                }

                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in reminder service: {ex.Message}");
            }
        }
    }
}

