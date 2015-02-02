using System.Data.Common;
using System.Data.Entity;
using AmbientDbContext.Interfaces;
using AmbientDbContext.Manager;

namespace DataModel
{
    public class BloggerDbContext : DbContext, IAmbientDbContext
    {
        public BloggerDbContext()
            : base("Blog")
        {
        }

        public BloggerDbContext(DbConnection connection, bool contextOwnsConnection)
            : base(connection, contextOwnsConnection)
        {

        }

        public DbSet<User> Users { get; set; }

        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Post> Posts { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbContextOption.Mode Mode { get; set; }
    }
}
