using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskScheduler.Application.DTOs.Auth;
using TaskScheduler.Application.DTOs.Users;
using TaskScheduler.Application.Services.Interfaces;
using TaskScheduler.Domain.Entities;
using TaskScheduler.Domain.Interfaces;

namespace TaskScheduler.Application.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IGoogleAuthService _googleAuthService;

        public AuthService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IEmailService emailService,
            IGoogleAuthService googleAuthService)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _emailService = emailService;
            _googleAuthService = googleAuthService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Check if email already exists
            if (await _unitOfWork.Users.EmailExistsAsync(dto.Email))
            {
                throw new InvalidOperationException("Email already registered");
            }

            var passwordHash = HashPassword(dto.Password);
            var verificationToken = GenerateToken();

            var user = new User
            {
                Email = dto.Email.ToLower(),
                PasswordHash = passwordHash,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                IsEmailVerified = false,
                EmailVerificationToken = verificationToken,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);

            // Create default notification settings
            var notificationSettings = new NotificationSettings
            {
                UserId = user.Id
            };
            await _unitOfWork.NotificationSettings.AddAsync(notificationSettings);

            await _unitOfWork.SaveChangesAsync();

            // Send verification email
            await _emailService.SendEmailVerificationAsync(user.Email, verificationToken);

            return new AuthResponseDto
            {
                Token = GenerateJwtToken(user),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);

            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }
            user.LastLoginAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = GenerateJwtToken(user),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<AuthResponseDto> GoogleLoginAsync(string idToken)
        {
            var googleUser = await _googleAuthService.ValidateGoogleTokenAsync(idToken);

            if (googleUser == null)
            {
                throw new UnauthorizedAccessException("Invalid Google token");
            }

            var user = await _unitOfWork.Users.GetByGoogleIdAsync(googleUser.GoogleId);

            if (user == null)
            {
                // Check if email exists
                user = await _unitOfWork.Users.GetByEmailAsync(googleUser.Email);

                if (user != null)
                {
                    // Link Google account to existing user
                    user.GoogleId = googleUser.GoogleId;
                    user.IsEmailVerified = googleUser.EmailVerified;
                }
                else
                {
                    // Create new user
                    user = new User
                    {
                        Email = googleUser.Email.ToLower(),
                        GoogleId = googleUser.GoogleId,
                        FirstName = googleUser.FirstName,
                        LastName = googleUser.LastName,
                        IsEmailVerified = googleUser.EmailVerified,
                        PasswordHash = string.Empty, // No password for Google users
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Users.AddAsync(user);

                    // Create default notification settings
                    var notificationSettings = new NotificationSettings
                    {
                        UserId = user.Id
                    };
                    await _unitOfWork.NotificationSettings.AddAsync(notificationSettings);
                }
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = GenerateJwtToken(user),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var user = await _unitOfWork.Users.GetByEmailVerificationTokenAsync(token);

            if (user == null)
            {
                return false;
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);

            if (user == null)
            {
                // Don't reveal if email exists
                return true;
            }

            var resetToken = GenerateToken();
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);

            await _unitOfWork.SaveChangesAsync();
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByPasswordResetTokenAsync(token);

            if (user == null)
            {
                return false;
            }

            user.PasswordHash = HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        public async Task<UserDto> GetCurrentUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }

        private string GenerateToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
