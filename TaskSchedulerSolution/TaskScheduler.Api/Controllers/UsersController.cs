using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskScheduler.Application.DTOs.Users;
using TaskScheduler.Application.Services.Interfaces;
using TaskScheduler.Domain.Entities;

namespace TaskScheduler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userService.GetUserByIdAsync(userId);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Only allow users to access their own data
                if (id != currentUserId)
                {
                    return Forbid();
                }

                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("me")]
        public async Task<ActionResult<UserDto>> UpdateCurrentUser([FromBody] UpdateUserDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var updatedUser = await _userService.UpdateUserAsync(userId, dto);
                return Ok(updatedUser);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("me")]
        public async Task<ActionResult> DeleteCurrentUser()
        {
            var userId = GetCurrentUserId();
            var result = await _userService.DeleteUserAsync(userId);

            if (result)
            {
                return Ok(new { message = "User deleted successfully" });
            }

            return BadRequest(new { message = "Failed to delete user" });
        }

        [HttpGet("me/notification-settings")]
        public async Task<ActionResult<NotificationSettings>> GetNotificationSettings()
        {
            var userId = GetCurrentUserId();
            var settings = await _userService.GetNotificationSettingsAsync(userId);
            return Ok(settings);
        }

        [HttpPut("me/notification-settings")]
        public async Task<ActionResult<NotificationSettings>> UpdateNotificationSettings([FromBody] NotificationSettings settings)
        {
            var userId = GetCurrentUserId();
            var updatedSettings = await _userService.UpdateNotificationSettingsAsync(userId, settings);
            return Ok(updatedSettings);
        }
    }
}
