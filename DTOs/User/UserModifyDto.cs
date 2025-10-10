using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs.User
{
    public class UserModifyDto
    {
        [Required]
        public int UserId { get; set; }
    }
}