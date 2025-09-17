using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs.User
{
    public class UserLoginDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}