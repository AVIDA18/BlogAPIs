namespace BlogApi.Models
{
    public class BlogLike
    {
        public int Id { get; set; }
        public bool Like { get; set; }
        public DateTime LikedAt { get; set; }

        //Liked on
        public int BlogId { get; set; }
        public Blog? Blog { get; set; }

        //Liked By
        public int UserId { get; set; }
        public User? User{ get; set; }
    }    
}