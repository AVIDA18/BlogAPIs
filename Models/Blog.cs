using System.ComponentModel.DataAnnotations;
using BlogApi.Models;

namespace BlogApi.Models
{
    public class Blog
    {
        public int Id { get; set; }
        [StringLength(600)]
        public string Title { get; set; } = string.Empty;
        [StringLength(600)]
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? BlogCategoryId { get; set; }
        public BlogCategory? BlogCategory { get; set; }
        // ONE blog --> MANY images
        public ICollection<BlogImages> Images { get; set; } = new List<BlogImages>();
        
        public int AuthorId { get; set; }
        public User? Author { get; set; }
    }
}