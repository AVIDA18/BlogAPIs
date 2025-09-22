namespace BlogApi.Models
{
    public class BlogCategory
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}