using System.Collections.Generic;
using System.Linq;
using AmbientDbContext.Manager;
using DataModel;
using TestExamples.ValueObjects;

namespace TestExamples.Operations
{
    public class BlogOperations
    {
        public BloggerDbContext BloggerDbContext
        {
            get
            {
                return DbContextLocator.GetDbContext<BloggerDbContext>();
            }
        }

        public VoBlog GetBlog(int blogId)
        {
            var blog = BloggerDbContext.Blogs.Find(blogId);
            return new VoBlog
            {
               Overview = blog.Overview,
               CreatedDateTime = blog.CreatedDate,
               PostId = blog.PostId
            };
        }

        public IEnumerable<VoBlog> GetBlogs()
        {
            var blogs = BloggerDbContext.Blogs;
            return blogs.Select(blog => new VoBlog
            {
                Overview = blog.Overview,
                CreatedDateTime = blog.CreatedDate,
                PostId = blog.PostId
            }).ToList();
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

        public VoBlog GetUserRecentBlog(long userId)
        {
            var recentBlog = BloggerDbContext.Blogs.OrderByDescending(blg => blg.CreatedDate).Select(blog => new VoBlog
            {
                CreatedDateTime = blog.CreatedDate,
                Overview = blog.Overview,
                PostId = blog.PostId
            }).First();
            return recentBlog;
        }
    }
}
