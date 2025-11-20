namespace TaskScheduler.Domain.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using TaskScheduler.Domain.Entities;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetCategoriesByUserIdAsync(int userId);
    Task<Category?> GetByNameAsync(int userId, string name);
    Task<bool> CategoryExistsAsync(int userId, string name);
}