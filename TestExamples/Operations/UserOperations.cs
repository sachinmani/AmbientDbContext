using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AmbientDbContext.Manager;
using DataModel;
using TestExamples.ValueObjects;

namespace TestExamples.Operations
{
    public class UserOperations
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

        public User GetUser(long userId)
        {
            var user = BloggerDbContext.Users.Single(usr => usr.UserId == userId);
            return user;
        }

        public async Task<User> GetUserAsync(long userId)
        {
            var user = await BloggerDbContext.Users.SingleAsync(usr => usr.UserId == userId);
            return user;
        }

        public User AddUser(VoUser user)
        {
            var newUser = new User
            {
                Name = user.Name,
                Occupation = user.Occupation
            };
            BloggerDbContext.Users.Add(newUser);
            return newUser;
        }

        public void UpdateBlogCreationFailureCount(long userId)
        {
            var user = BloggerDbContext.Users.Find(userId);
            var count = user.BlogPostCreationFailureCount;
            user.BlogPostCreationFailureCount = count + 1;
        }
    }
}
