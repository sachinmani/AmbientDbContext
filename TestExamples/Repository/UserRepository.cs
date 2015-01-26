using System.Threading.Tasks;
using AmbientDbContext.Manager;
using DataModel;
using TestExamples.Interfaces;
using TestExamples.Operations;
using TestExamples.ValueObjects;

namespace TestExamples.Repository
{
    public class UserRepository : IUserRepository
    {
        private UserOperations _userOperations;
        public UserRepository(UserOperations userOperations)
        {
            _userOperations = userOperations;
        }
        public VoUser GetUser(long userId)
        {
            using (var dbContextScope = DbContextScopeFactory.CreateAmbientDbContextinReadonlyMode<BloggerDbContext>())
            {
                return _userOperations.GetUser(userId);
            }
        }

        public async Task<VoUser> GetUserAsync(long userId)
        {
            using (var dbContextScope = DbContextScopeFactory.CreateAmbientDbContextinReadonlyMode<BloggerDbContext>())
            {
                return await _userOperations.GetUserAsync(userId);
            }
        }

        public long AddUser(VoUser user)
        {
            using (var dbContextScope = DbContextScopeFactory.CreateAmbientDbContextinTransactionMode<BloggerDbContext>())
            {
                var addedUser = _userOperations.AddUser(user);
                dbContextScope.SaveChanges();
                return addedUser.UserId;
            }
        }
    }
}
