namespace BlogApi.DTOs.BlogCategory
{
    public class BlogCategoryListDto
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}