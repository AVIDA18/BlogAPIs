using BlogApis.Models;

namespace BlogApi.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? BlogCategoryId { get; set; }
        public BlogCategory? BlogCategory { get; set; }
        // ONE blog --> MANY images
        public ICollection<BlogImage> Images { get; set; } = new List<BlogImage>();
        
        public int AuthorId { get; set; }
        public User? Author { get; set; }
    }
}