using System.Collections.Generic;
using AmbientDbContext.Manager;
using DataModel;
using TestExamples.Interfaces;
using TestExamples.Operations;
using TestExamples.ValueObjects;

namespace TestExamples.Repository
{
    public class BlogRepository : IBlogRepository
    {
        private readonly BlogOperations _blogOperations;
        private readonly PostOperations _postOperations;

        public BlogRepository(BlogOperations blogOperations, PostOperations postOperations)
        {
            _blogOperations = blogOperations;
            _postOperations = postOperations;
        }

        public VoBlog GetBlog(int blogId)
        {
            using (DbContextScopeFactory.CreateAmbientDbContextinReadonlyMode<BloggerDbContext>())
            {
                var blog = _blogOperations.GetBlog(blogId);
                var post = _postOperations.GetPost(blog.PostId);
                blog.Post = post;
                return blog;
            }
        }

        public IEnumerable<VoBlog> GetBlogs()
        {
            using (DbContextScopeFactory.CreateAmbientDbContextinReadonlyMode<BloggerDbContext>())
            {
                var blogs =  _blogOperations.GetBlogs();
                return blogs;
            }
        }


        public void AddBlog(VoBlog voBlog)
        {
            using (var dbContextScope = DbContextScopeFactory.CreateAmbientDbContextinTransactionMode<BloggerDbContext>())
            {
                var blog = _blogOperations.AddBlog(voBlog);
                var post = _postOperations.AddPost(voBlog.Post);
                blog.BlogPost = post;
                dbContextScope.SaveChanges();
            }
        }


        public VoBlog GetUserRecentBlog(long userId)
        {
            using (var dbContextScope = DbContextScopeFactory.CreateAmbientDbContextinReadonlyMode<BloggerDbContext>())
            {
                var userRecentBlog = _blogOperations.GetUserRecentBlog(userId);
                var post = _postOperations.GetPost(userRecentBlog.PostId);
                userRecentBlog.Post = post;
                return userRecentBlog;
            }
        }
    }
}
