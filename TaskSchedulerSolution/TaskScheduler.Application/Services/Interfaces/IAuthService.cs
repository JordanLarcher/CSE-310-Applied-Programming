using System.Threading.Tasks;
using TaskScheduler.Application.DTOs.Auth;
using TaskScheduler.Application.DTOs.Users;

namespace TaskScheduler.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> GoogleLoginAsync(string idToken);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> RequestPasswordResetAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<UserDto> GetCurrentUserAsync(int userId);
    }
}
