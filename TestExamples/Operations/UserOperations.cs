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
        public BloggerDbContext BloggerDbContext
        {
            get
            {
                //DbContextLocator locate and return the AmbientDbContext if one exists  
                return DbContextLocator.GetDbContext<BloggerDbContext>();
            }
        }

        public VoUser GetUser(long userId)
        {
            var user = BloggerDbContext.Users.Single(usr => usr.UserId == userId);
            return new VoUser
            {
                Name = user.Name,
                Occupation = user.Occupation
            };
        }

        public async Task<VoUser> GetUserAsync(long userId)
        {
            var user = await BloggerDbContext.Users.SingleAsync(usr => usr.UserId == userId);
            return new VoUser
            {
                Name = user.Name,
                Occupation = user.Occupation
            };
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
    }
}
