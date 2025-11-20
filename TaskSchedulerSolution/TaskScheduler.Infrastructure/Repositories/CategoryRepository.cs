using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskScheduler.Domain.Entities;
using TaskScheduler.Domain.Interfaces;
using TaskScheduler.Infrastructure.Data;

namespace TaskScheduler.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Category>> GetCategoriesByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByNameAsync(int userId, string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.UserId == userId &&
                                          c.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> CategoryExistsAsync(int userId, string name)
        {
            return await _dbSet
                .AnyAsync(c => c.UserId == userId &&
                               c.Name.ToLower() == name.ToLower());
        }
    }
}