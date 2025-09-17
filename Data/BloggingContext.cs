using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data
{
    public partial class BloggingContext : DbContext
    {
        public BloggingContext(DbContextOptions<BloggingContext> options) : base(options) { }

        public DbSet<ApiLog> ApiLogs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }
        public DbSet<BlogLike> BlogLikes { get; set; }

        //Study this properly.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Blog>()
                .HasOne(b => b.Author)                  //Each blog can have one author
                .WithMany()                             //Each author can have multiple blog
                .HasForeignKey(b => b.AuthorId)         //Blog has fk authorId
                .OnDelete(DeleteBehavior.Restrict);     //If author is tried to delete restrict delete.

            modelBuilder.Entity<BlogComment>()
                .HasOne(c => c.Blog)                    //Each blogcomment can have one blog
                .WithMany()                             //Each blog can have multiple blog comment
                .HasForeignKey(c => c.BlogId)           //blogComment has fk blogId
                .OnDelete(DeleteBehavior.Cascade);      //Delete all related comments if a blog is deleted

            modelBuilder.Entity<BlogComment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Create a unique index for the combination of BlogId and UserId in BlogLikes
            modelBuilder.Entity<BlogLike>()
                .HasIndex(bl => new { bl.BlogId, bl.UserId })
                .IsUnique();

            modelBuilder.Entity<BlogLike>()
                .HasOne(c => c.Blog)
                .WithMany()
                .HasForeignKey(c => c.BlogId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BlogLike>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);                
        }
    }
}