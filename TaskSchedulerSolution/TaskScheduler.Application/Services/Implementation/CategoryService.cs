using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskScheduler.Application.Services.Interfaces;
using TaskScheduler.Domain.Entities;
using TaskScheduler.Domain.Interfaces;

namespace TaskScheduler.Application.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Category> CreateCategoryAsync(int userId, string name, string color)
        {
            if (await _unitOfWork.Categories.CategoryExistsAsync(userId, name))
            {
                throw new InvalidOperationException($"Category '{name}' already exists");
            }

            var category = new Category
            {
                UserId = userId,
                Name = name,
                Color = color,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return category;
        }

        public async Task<Category> UpdateCategoryAsync(int userId, int categoryId, string name, string color)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);

            if (category == null || category.UserId != userId)
            {
                throw new InvalidOperationException("Category not found or access denied");
            }

            if (category.Name != name && await _unitOfWork.Categories.CategoryExistsAsync(userId, name))
            {
                throw new InvalidOperationException($"Category '{name}' already exists");
            }

            category.Name = name;
            category.Color = color;

            await _unitOfWork.SaveChangesAsync();

            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int userId, int categoryId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);

            if (category == null || category.UserId != userId)
            {
                return false;
            }

            await _unitOfWork.Categories.DeleteAsync(categoryId);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<Category?> GetCategoryByIdAsync(int userId, int categoryId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);

            if (category == null || category.UserId != userId)
            {
                return null;
            }

            return category;
        }

        public async Task<IEnumerable<Category>> GetUserCategoriesAsync(int userId)
        {
            return await _unitOfWork.Categories.GetCategoriesByUserIdAsync(userId);
        }
    }
}
