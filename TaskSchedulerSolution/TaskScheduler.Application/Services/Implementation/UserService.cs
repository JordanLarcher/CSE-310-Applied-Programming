using System;
using System.Threading.Tasks;
using TaskScheduler.Application.DTOs.Users;
using TaskScheduler.Application.Services.Interfaces;
using TaskScheduler.Domain.Entities;
using TaskScheduler.Domain.Interfaces;

namespace TaskScheduler.Application.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        public async Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
            {
                user.FirstName = dto.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(dto.LastName))
            {
                user.LastName = dto.LastName;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _unitOfWork.Users.EmailExistsAsync(dto.Email))
                {
                    throw new InvalidOperationException("Email already in use");
                }

                user.Email = dto.Email.ToLower();
                user.IsEmailVerified = false; // Require re-verification
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetUserByIdAsync(userId);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            await _unitOfWork.Users.DeleteAsync(userId);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<NotificationSettings> GetNotificationSettingsAsync(int userId)
        {
            var settings = await _unitOfWork.NotificationSettings.GetByUserIdAsync(userId);

            if (settings == null)
            {
                // Create default settings
                settings = new NotificationSettings
                {
                    UserId = userId
                };
                await _unitOfWork.NotificationSettings.AddAsync(settings);
                await _unitOfWork.SaveChangesAsync();
            }

            return settings;
        }

        public async Task<NotificationSettings> UpdateNotificationSettingsAsync(int userId, NotificationSettings newSettings)
        {
            var settings = await _unitOfWork.NotificationSettings.GetByUserIdAsync(userId);

            if (settings == null)
            {
                newSettings.UserId = userId;
                await _unitOfWork.NotificationSettings.AddAsync(newSettings);
            }
            else
            {
                settings.EmailReminders = newSettings.EmailReminders;
                settings.OverdueAlerts = newSettings.OverdueAlerts;
                settings.TaskUpdates = newSettings.TaskUpdates;
                settings.DailyDigest = newSettings.DailyDigest;
                settings.WeeklySummary = newSettings.WeeklySummary;
                settings.DailyDigestTime = newSettings.DailyDigestTime;
                settings.WeeklySummaryDay = newSettings.WeeklySummaryDay;
                settings.UpdatedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetNotificationSettingsAsync(userId);
        }
    }
}
