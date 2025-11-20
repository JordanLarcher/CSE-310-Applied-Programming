using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskScheduler.Application.Services.Interfaces;

namespace TaskScheduler.Infrastructure.ExternalServices
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            _smtpUsername = _configuration["Smtp:Username"] ?? "";
            _smtpPassword = _configuration["Smtp:Password"] ?? "";
            _fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@taskscheduler.com";
            _fromName = _configuration["Smtp:FromName"] ?? "TaskScheduler";
        }

        public async Task SendTaskCreatedEmailAsync(string email, Domain.Entities.Task task)
        {
            var subject = "New Task Created";
            var body = $@"
                <h2>Task Created Successfully</h2>
                <p><strong>Title:</strong> {task.Title}</p>
                <p><strong>Description:</strong> {task.Description}</p>
                <p><strong>Due Date:</strong> {task.DueDate?.ToString("MMM dd, yyyy")}</p>
                <p><strong>Priority:</strong> {task.Priority}</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendTaskUpdatedEmailAsync(string email, Domain.Entities.Task task)
        {
            var subject = "Task Updated";
            var body = $@"
                <h2>Task Updated</h2>
                <p><strong>Title:</strong> {task.Title}</p>
                <p><strong>Status:</strong> {task.Status}</p>
                <p><strong>Due Date:</strong> {task.DueDate?.ToString("MMM dd, yyyy")}</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendOverdueTaskEmailAsync(string email, Domain.Entities.Task task)
        {
            var subject = "⚠️ Task Overdue";
            var body = $@"
                <h2 style='color: red;'>Task is Overdue</h2>
                <p><strong>Title:</strong> {task.Title}</p>
                <p><strong>Due Date:</strong> {task.DueDate?.ToString("MMM dd, yyyy")}</p>
                <p>Please complete this task as soon as possible.</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendTaskReminderEmailAsync(string email, Domain.Entities.Task task)
        {
            var subject = "🔔 Task Reminder";
            var body = $@"
                <h2>Task Reminder</h2>
                <p><strong>Title:</strong> {task.Title}</p>
                <p><strong>Description:</strong> {task.Description}</p>
                <p><strong>Due Date:</strong> {task.DueDate?.ToString("MMM dd, yyyy HH:mm")}</p>
                <p><strong>Priority:</strong> {task.Priority}</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName)
        {
            var subject = "Welcome to TaskScheduler!";
            var body = $@"
                <h2>Welcome, {firstName}!</h2>
                <p>Thank you for joining TaskScheduler. We're excited to help you stay organized and productive.</p>
                <p>Get started by creating your first task!</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            var resetLink = $"{_configuration["AppUrl"]}/reset-password?token={resetToken}";
            var subject = "Password Reset Request";
            var body = $@"
                <h2>Password Reset</h2>
                <p>You requested a password reset. Click the link below to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you didn't request this, please ignore this email.</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEmailVerificationAsync(string email, string verificationToken)
        {
            var verificationLink = $"{_configuration["AppUrl"]}/verify-email?token={verificationToken}";
            var subject = "Verify Your Email";
            var body = $@"
                <h2>Email Verification</h2>
                <p>Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationLink}'>Verify Email</a></p>
                <p>If you didn't create an account, please ignore this email.</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, _fromName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(_smtpHost, _smtpPort);
                smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                smtpClient.EnableSsl = true;

                await smtpClient.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}: {ex.Message}");
                throw;
            }
        }
    }
}
