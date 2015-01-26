using AmbientDbContext.Manager;
using DataModel;
using TestExamples.ValueObjects;

namespace TestExamples.Operations
{
    public class PostOperations
    {   

        public BloggerDbContext BloggerDbContext
        {
            get
            {
                return DbContextLocator.GetDbContext<BloggerDbContext>();
            }
        }

        public VoPost GetPost(long postId)
        {
            var post = BloggerDbContext.Posts.Find(postId);
            return new VoPost
            {
                Content = post.Content,
                Meta = post.Meta,
                ShortDescription = post.ShortDescription,
                Title = post.Title
            };
        }

        public Post AddPost(VoPost vopost)
        {
            var post = new Post
            {
                Content = vopost.Content,
                Meta = vopost.Meta,
                ShortDescription = vopost.ShortDescription,
                Title = vopost.Title
            };
            BloggerDbContext.Posts.Add(post);
            return post;
        }
    }
}
