using System.Threading.Tasks;

namespace TaskScheduler.Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendTaskCreatedEmailAsync(string email, Domain.Entities.Task task);
        Task SendTaskUpdatedEmailAsync(string email, Domain.Entities.Task task);
        Task SendOverdueTaskEmailAsync(string email, Domain.Entities.Task task);
        Task SendTaskReminderEmailAsync(string email, Domain.Entities.Task task);
        Task SendWelcomeEmailAsync(string email, string firstName);
        Task SendPasswordResetEmailAsync(string email, string resetToken);
        Task SendEmailVerificationAsync(string email, string verificationToken);
    }
}
