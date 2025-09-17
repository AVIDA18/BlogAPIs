namespace BlogApi.Models
{
    public class ApiLog
    {
        public int Id { get; set; }
        public string Api { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}