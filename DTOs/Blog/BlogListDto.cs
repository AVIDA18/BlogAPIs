using BlogApi.DTOs.BlogCategory;
using BlogApi.DTOs.Images;
using BlogApi.DTOs.User;

namespace BlogApi.DTOs.Blog
{
    public class BlogListDto
    {
        public int Id {get; set;}
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ActualAuthor{get; set;}
        public string? Slug{get; set;}
        public string? Source{get; set;}
        public DateTime? BlogDate { get; set; }

        public UserListDto Users { get; set; }
        public BlogCategoryListDto Category { get; set; }
        public List<ImagesListDto> Images { get; set; }
    }
}