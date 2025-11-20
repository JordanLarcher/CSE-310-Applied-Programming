using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskScheduler.Application.DTOs.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace TaskScheduler.Client.Services
{
    public class AuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthenticationStateProvider _authStateProvider;

        public AuthenticationService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = (CustomAuthenticationStateProvider)authStateProvider;
        }

        public async Task<AuthResult> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResult>();

                    if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
                    {
                        await _authStateProvider.MarkUserAsAuthenticated(result.Token);
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                        return result;
                    }
                }

                var errorResult = await response.Content.ReadFromJsonAsync<AuthResult>();
                return errorResult ?? AuthResult.FailureResult("Login failed. Please try again.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return AuthResult.FailureResult("An error occurred during login.");
            }
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerDto);

                if (response.IsSuccessStatusCode)
                {
                    return AuthResult.SuccessResult("", "", "", "", DateTime.UtcNow);
                }

                var errorResult = await response.Content.ReadFromJsonAsync<AuthResult>();
                return errorResult ?? AuthResult.FailureResult("Registration failed. Please try again.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return AuthResult.FailureResult("An error occurred during registration.");
            }
        }

        public async Task LogoutAsync()
        {
            await _authStateProvider.MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task ForgotPasswordAsync(string email)
        {
            await _httpClient.PostAsJsonAsync("api/auth/forgot-password", new { Email = email });
        }

        public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", resetPasswordDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _authStateProvider.GetTokenAsync();
        }
    }
}
