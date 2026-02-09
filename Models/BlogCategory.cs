using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models
{
    public class BlogCategory
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}