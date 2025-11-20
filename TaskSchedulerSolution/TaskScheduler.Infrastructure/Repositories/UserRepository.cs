using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskScheduler.Domain.Entities;
using TaskScheduler.Domain.Interfaces;
using TaskScheduler.Infrastructure.Data;

namespace TaskScheduler.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(u => u.NotificationSettings)
                .FirstOrDefaultAsync(u => u.Email == email.ToLower());
        }

        public async Task<User?> GetByGoogleIdAsync(string googleId)
        {
            return await _dbSet
                .Include(u => u.NotificationSettings)
                .FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task<User?> GetByEmailVerificationTokenAsync(string token)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
        }

        public async Task<User?> GetByPasswordResetTokenAsync(string token)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token &&
                                          u.PasswordResetTokenExpiry > System.DateTime.UtcNow);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email.ToLower());
        }

        public override async Task<User?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(u => u.NotificationSettings)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}