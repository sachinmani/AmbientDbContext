using System.Collections.Generic;
using System.Linq;
using AmbientDbContext.Manager;
using DataModel;
using TestExamples.ValueObjects;

namespace TestExamples.Operations
{
    public class BlogOperations
    {
        private BloggerDbContext _bloggerDbContext;
        public BloggerDbContext BloggerDbContext
        {
            get
            {
                //DbContextLocator locate and return the AmbientDbContext if one exists  
                _bloggerDbContext = DbContextLocator.GetDbContext<BloggerDbContext>();
                return _bloggerDbContext;
            }
            set
            {
                //set the dbcontext manually when using non ambient transction
                _bloggerDbContext = value;
            }
        }

        public Blog GetBlog(int blogId)
        {
            var blog = BloggerDbContext.Blogs.Find(blogId);
            return blog;
        }

        public IEnumerable<Blog> GetBlogs()
        {
            var blogs = BloggerDbContext.Blogs;
            return blogs;
        }

        public Blog AddBlog(VoBlog voBlog)
        {
            var blog = new Blog
            {
                CreatedDate = voBlog.CreatedDateTime,
                UpdatedDate = voBlog.CreatedDateTime,
                Overview = voBlog.Overview,
                UserId = voBlog.UserId
            };
            BloggerDbContext.Blogs.Add(blog);
            return blog;
        }

        public Blog GetUserRecentBlog(long userId)
        {
            var recentBlog = BloggerDbContext.Blogs.OrderByDescending(blg => blg.CreatedDate).First();
            return recentBlog;
        }
    }
}
