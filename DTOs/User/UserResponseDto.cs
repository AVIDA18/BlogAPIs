namespace BlogApi.DTOs.User
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string? ProfileImagePath { get; set; }
        public string Role { get; set; } = "User";
    }
}
