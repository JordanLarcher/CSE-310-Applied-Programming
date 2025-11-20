using Microsoft.AspNetCore.Mvc;
using TaskScheduler.Application.DTOs.Auth;
using TaskScheduler.Application.Services.Interfaces;

namespace TaskScheduler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var response = await _authService.RegisterAsync(dto);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            try
            {
                var response = await _authService.LoginAsync(dto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("google")]
        public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] string idToken)
        {
            try
            {
                var response = await _authService.GoogleLoginAsync(idToken);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("verify-email")]
        public async Task<ActionResult> VerifyEmail([FromQuery] string token)
        {
            var result = await _authService.VerifyEmailAsync(token);

            if (result)
            {
                return Ok(new { message = "Email verified successfully" });
            }

            return BadRequest(new { message = "Invalid or expired verification token" });
        }

        [HttpPost("request-password-reset")]
        public async Task<ActionResult> RequestPasswordReset([FromBody] string email)
        {
            await _authService.RequestPasswordResetAsync(email);
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromQuery] string token, [FromBody] string newPassword)
        {
            var result = await _authService.ResetPasswordAsync(token, newPassword);

            if (result)
            {
                return Ok(new { message = "Password reset successfully" });
            }

            return BadRequest(new { message = "Invalid or expired reset token" });
        }
    }
}
