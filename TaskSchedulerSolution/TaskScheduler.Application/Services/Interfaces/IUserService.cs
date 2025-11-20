using System.Threading.Tasks;
using TaskScheduler.Application.DTOs.Users;
using TaskScheduler.Domain.Entities;

namespace TaskScheduler.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(int userId);
        Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto dto);
        Task<bool> DeleteUserAsync(int userId);
        Task<NotificationSettings> GetNotificationSettingsAsync(int userId);
        Task<NotificationSettings> UpdateNotificationSettingsAsync(int userId, NotificationSettings settings);
    }
}
