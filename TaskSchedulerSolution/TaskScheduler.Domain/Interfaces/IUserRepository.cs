using System.Threading.Tasks;
using TaskScheduler.Domain.Entities;

namespace TaskScheduler.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByGoogleIdAsync(string googleId);
        Task<User?> GetByEmailVerificationTokenAsync(string token);
        Task<User?> GetByPasswordResetTokenAsync(string token);
        Task<bool> EmailExistsAsync(string email);
    }
}
