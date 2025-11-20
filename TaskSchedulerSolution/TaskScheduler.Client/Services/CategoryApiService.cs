using System.Net.Http.Json;
using TaskScheduler.Domain.Entities;

namespace TaskScheduler.Client.Services;

public class CategoryApiService
{
    private readonly HttpClient _httpClient;

    public CategoryApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/categories");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Category>>() ?? new List<Category>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching categories: {ex.Message}");
            return new List<Category>();
        }
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/categories/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Category>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching category: {ex.Message}");
            return null;
        }
    }

    public async Task<Category?> CreateCategoryAsync(CreateCategoryDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/categories", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Category>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating category: {ex.Message}");
            return null;
        }
    }

    public async Task<Category?> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/categories/{id}", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Category>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating category: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/categories/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting category: {ex.Message}");
            return false;
        }
    }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";
}
