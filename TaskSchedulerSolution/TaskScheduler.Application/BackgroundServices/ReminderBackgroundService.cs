using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskScheduler.Application.Services.Interfaces;

namespace TaskScheduler.Application.BackgroundServices
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly ILogger<ReminderBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ReminderBackgroundService(
            ILogger<ReminderBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reminder Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    _logger.LogInformation("Processing pending notifications at {Time}", DateTime.UtcNow);
                    await notificationService.ProcessPendingNotificationsAsync();

                    // Wait for 1 minute before checking again
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Reminder Background Service: {Message}", ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Wait 30 seconds before retry
                }
            }

            _logger.LogInformation("Reminder Background Service is stopping.");
        }
    }
}
