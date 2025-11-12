using TaskScheduler.Shared; // <-- Import our shared models

namespace TaskScheduler.Api.Services
{
    // This class inherits from BackgroundService, which is the standard
    // way to create long-running background tasks in ASP.NET Core.
    public class ReminderService : BackgroundService
    {
        private readonly ILogger<ReminderService> _logger;

        // We use an IServiceProvider to get access to the (singleton) task list.
        // This is a more advanced concept (Dependency Injection) but allows
        // us to access the same list as our TasksController.
        // A simpler way for this module is to make the list public static.
        // Let's stick to the static list from the controller for simplicity.

        public ReminderService(ILogger<ReminderService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reminder Service is starting.");

            // REQUIREMENT: Loops
            // This 'while' loop runs forever until the application is stopped.
            // The 'stoppingToken.IsCancellationRequested' is the signal to stop.
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Reminder Service is checking for tasks...");
                await CheckForReminders();

                // Wait for 1 minute before checking again
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("Reminder Service is stopping.");
        }

        private async Task CheckForReminders()
        {
            // Access the static list from our controller
            // Note: This isn't best practice for a real app (we'd use a
            // shared service), but it's the simplest way to get it working.
            var tasks = typeof(Controllers.TasksController)
                .GetField("_tasks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
                .GetValue(null) as List<TaskItem>;

            if (tasks == null)
            {
                _logger.LogError("Could not access task list.");
                return;
            }

            // We must 'lock' the list to prevent errors if the API
            // tries to change it at the same time.
            lock (tasks)
            {
                // REQUIREMENT: Loops
                // This 'foreach' loop iterates over every task in the list.
                foreach (var task in tasks)
                {
                    // REQUIREMENT: Expressions
                    // This complex boolean expression checks all conditions for a reminder.
                    bool shouldSendReminder =
                        task.ReminderTime.HasValue &&
                        task.ReminderTime.Value <= DateTime.UtcNow &&
                        !task.IsComplete &&
                        !task.IsReminderSent &&
                        task.Config.LogToFile; // Check our struct property!

                    // REQUIREMENT: Conditionals
                    if (shouldSendReminder)
                    {
                        _logger.LogWarning($"Reminder triggered for task: {task.Title}");
                        
                        // Mark as sent so we don't send it again
                        task.IsReminderSent = true; 

                        // Trigger the file write
                        WriteReminderToFile(task);
                    }
                }
            }
             // 'await' is not needed here as WriteReminderToFile is synchronous
             // for simplicity, but we'll make the method async for correctness
             await Task.CompletedTask;
        }

        // REQUIREMENT: Read and write to a file
        private void WriteReminderToFile(TaskItem task)
        {
            try
            {
                // REQUIREMENT: Expressions
                // This is a string formatting expression (string interpolation).
                string logMessage = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] " +
                                  $"REMINDER: Task '{task.Title}' is due at {task.DueDate:g}.\n";

                // This appends the text to a file.
                // If the file doesn't exist, it creates it.
                // This is our "notification".
                File.AppendAllText("reminder_log.txt", logMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to write reminder to file for task {task.Id}");
            }
        }
    }
}
