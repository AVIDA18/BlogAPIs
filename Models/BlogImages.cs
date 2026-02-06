using System.Text.Json.Serialization;
using BlogApi.Models;

namespace BlogApis.Models
{
    public class BlogImages
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        [JsonIgnore]
        public Blog? Blog { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltTxt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}