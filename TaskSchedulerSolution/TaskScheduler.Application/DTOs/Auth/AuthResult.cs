namespace TaskScheduler.Application.DTOs.Auth
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string> Errors { get; set; } = new();

        public static AuthResult SuccessResult(string token, string email, string firstName, string lastName, DateTime expiresAt)
        {
            return new AuthResult
            {
                Success = true,
                Token = token,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                ExpiresAt = expiresAt
            };
        }

        public static AuthResult FailureResult(params string[] errors)
        {
            return new AuthResult
            {
                Success = false,
                Errors = errors.ToList()
            };
        }
    }
}
