using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs.User
{
    public class UserRegisterDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        public string? Website { get; set; } = string.Empty;
        public IFormFile? ProfileImage { get; set; }
    }
}