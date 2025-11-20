using System.Collections.Generic;
using System.Threading.Tasks;
using TaskScheduler.Domain.Entities;

namespace TaskScheduler.Application.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Category> CreateCategoryAsync(int userId, string name, string color);
        Task<Category> UpdateCategoryAsync(int userId, int categoryId, string name, string color);
        Task<bool> DeleteCategoryAsync(int userId, int categoryId);
        Task<Category?> GetCategoryByIdAsync(int userId, int categoryId);
        Task<IEnumerable<Category>> GetUserCategoriesAsync(int userId);
    }
}
