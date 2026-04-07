namespace BlogApi.DTOs.User
{
    public class UserListDto
    {
        public int Id{get; set;}
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? ProfileImagePath { get; set; }
    }
}