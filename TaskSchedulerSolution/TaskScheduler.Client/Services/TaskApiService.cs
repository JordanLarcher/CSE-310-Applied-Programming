using System.Net.Http.Json;
using TaskScheduler.Application.DTOs.Tasks;

namespace TaskScheduler.Client.Services
{
    public class TaskApiService
    {
        private readonly HttpClient _httpClient;

        public TaskApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TaskDto>> GetAllTasksAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TaskDto>>("api/tasks") ?? new List<TaskDto>();
        }

        public async Task<TaskDto?> GetTaskByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<TaskDto>($"api/tasks/{id}");
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/tasks", createTaskDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TaskDto>() ?? throw new Exception("Failed to create task");
        }

        public async Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/tasks/{id}", updateTaskDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TaskDto>() ?? throw new Exception("Failed to update task");
        }

        public async Task DeleteTaskAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/tasks/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<TaskDto>> GetTasksByCategoryAsync(int categoryId)
        {
            return await _httpClient.GetFromJsonAsync<List<TaskDto>>($"api/tasks/category/{categoryId}") ?? new List<TaskDto>();
        }

        public async Task<List<TaskDto>> GetOverdueTasksAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TaskDto>>("api/tasks/overdue") ?? new List<TaskDto>();
        }
    }
}
