using AmbientDbContext.Manager;
using DataModel;
using TestExamples.ValueObjects;

namespace TestExamples.Operations
{
    public class PostOperations
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

        public Post GetPost(long postId)
        {
            var post = BloggerDbContext.Posts.Find(postId);
            return post;
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
