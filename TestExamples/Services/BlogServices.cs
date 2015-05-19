using System;
using System.Collections.Generic;
using System.Linq;
using AmbientDbContext.Manager;
using DataModel;
using TestExamples.Interfaces;
using TestExamples.Operations;
using TestExamples.ValueObjects;

namespace TestExamples.Services
{
    public class BlogServices : IBlogServices
    {
        private readonly BlogOperations _blogOperations;
        private readonly PostOperations _postOperations;
        private readonly UserOperations _userOperations;

        public BlogServices()
        {
            _blogOperations = new BlogOperations();
            _postOperations = new PostOperations();
            _userOperations = new UserOperations();
        }

        public VoBlog GetBlog(int blogId)
        {
            //Demonstration of creating an AmbientDbContext in readonly mode
            using (new DbContextScopeFactory().CreateAmbientDbContextInReadonlyMode<BloggerDbContext>())
            {
                var blog = _blogOperations.GetBlog(blogId);
                var voBlog = new VoBlog
                {
                    Overview = blog.Overview,
                    CreatedDateTime = blog.CreatedDate,
                    PostId = blog.PostId
                };
                var post = _postOperations.GetPost(blog.PostId);
                voBlog.Post = new VoPost
                {
                    Content = post.Content,
                    Meta = post.Meta,
                    ShortDescription = post.ShortDescription,
                    Title = post.Title
                };
                return voBlog;
            }
        }

        public IEnumerable<VoBlog> GetBlogs()
        {
            //Demonstration of creating an AmbientDbContext in readonly mode
            using (new DbContextScopeFactory().CreateAmbientDbContextInReadonlyMode<BloggerDbContext>())
            {
                var blogs =  _blogOperations.GetBlogs();
                return blogs.Select(blog => new VoBlog
                {
                    Overview = blog.Overview,
                    CreatedDateTime = blog.CreatedDate,
                    PostId = blog.PostId
                }).ToList();
            }
        }


        public void AddBlog(VoBlog voBlog)
        {
            //Demonstration of creating an AmbientDbContext in transaction mode
            using (var dbContextScope = new DbContextScopeFactory().CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
            {
                bool exceptionOccured = false;
                var user = _userOperations.GetUser(voBlog.UserId);
                try
                {
                    Console.WriteLine("BlogPostCreationFailureCount :" +user.BlogPostCreationFailureCount);
                    var blog = _blogOperations.AddBlog(voBlog);
                    var post = _postOperations.AddPost(voBlog.Post);
                    blog.BlogPost = post;
                    dbContextScope.SaveChanges();
                    dbContextScope.Commit();
                }
                catch (Exception)
                {
                    exceptionOccured = true;
                    throw;
                }
                finally
                {
                    if (exceptionOccured)
                    {
                        var userOperations = new UserOperations();
                        using (
                            var nonAmbientdbContextScope =
                                new DbContextScopeFactory().CreateNonAmbientDbContextInTransactionMode<BloggerDbContext>())
                        {
                            //Setting the user operations dbContext with the dbContext from DbContextScope just created
                            //else we would be using ambient DbContext.
                            userOperations.UpdateBlogCreationFailureCount(voBlog.UserId);

                            var updateduser = userOperations.GetUser(voBlog.UserId);
                            nonAmbientdbContextScope.RefreshParentCacheWithUpdatedEntities(new List<User> { updateduser });
                            Console.WriteLine("BlogPostCreationFailureCount :" + user.BlogPostCreationFailureCount);
                        }
                    }
                }
            }
        }


        public VoBlog GetUserRecentBlog(long userId)
        {
            //Demonstration of creating an AmbientDbContext in readonly mode
            using (new DbContextScopeFactory().CreateAmbientDbContextInReadonlyMode<BloggerDbContext>())
            {
                var userRecentBlog = _blogOperations.GetUserRecentBlog(userId);
                var blogDto = new VoBlog
                {
                    CreatedDateTime = userRecentBlog.CreatedDate,
                    Overview = userRecentBlog.Overview,
                    PostId = userRecentBlog.PostId
                };
                var post = _postOperations.GetPost(userRecentBlog.PostId);
                blogDto.Post = new VoPost
                {
                    Content = post.Content,
                    Meta = post.Meta,
                    ShortDescription = post.ShortDescription,
                    Title = post.Title
                };
                return blogDto;
            }
        }
    }
}
