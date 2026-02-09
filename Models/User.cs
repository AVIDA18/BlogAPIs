using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models
{
    public class User
    {
        public int Id { get; set; }
        [StringLength(30)]
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        [StringLength(50)]
        public string Email { get; set; } = string.Empty;
        [StringLength(30)]
        public string? Website { get; set; }
        public string? ProfileImagePath { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        //Email
        public bool EmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpires { get; set; }
    }
}