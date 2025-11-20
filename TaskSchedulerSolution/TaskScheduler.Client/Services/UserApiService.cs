using System.Net.Http.Json;
using TaskScheduler.Application.DTOs.Users;

namespace TaskScheduler.Client.Services
{
    public class UserApiService
    {
        private readonly HttpClient _httpClient;

        public UserApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserProfileDto?> GetProfileAsync()
        {
            return await _httpClient.GetFromJsonAsync<UserProfileDto>("api/users/profile");
        }

        public async Task<UserDto> UpdateProfileAsync(UserDto userDto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/users/profile", userDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>() ?? throw new Exception("Failed to update profile");
        }
    }
}
