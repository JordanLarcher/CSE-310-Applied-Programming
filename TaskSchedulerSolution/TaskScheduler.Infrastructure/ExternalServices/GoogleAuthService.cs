using System;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskScheduler.Application.Services.Interfaces;

namespace TaskScheduler.Infrastructure.ExternalServices
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<GoogleUserInfo> ValidateGoogleTokenAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                if (payload == null)
                {
                    _logger.LogWarning("Google token validation returned null payload");
                    return null;
                }

                return new GoogleUserInfo
                {
                    GoogleId = payload.Subject,
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    Picture = payload.Picture,
                    EmailVerified = payload.EmailVerified
                };
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogError(ex, "Invalid Google JWT token");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Google token");
                throw;
            }
        }
    }
}