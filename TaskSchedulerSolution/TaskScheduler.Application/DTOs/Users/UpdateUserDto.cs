using System.ComponentModel.DataAnnotations;

namespace TaskScheduler.Application.DTOs.Users
{
    public class UpdateUserDto
    {
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string? FirstName { get; set; }

        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string? LastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
    }
}
