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
            //We are creating a dbContextScope here which in turn creates an AmbientDbContext
            using (var dbContextScope = DbContextScopeFactory.CreateAmbientDbContextinReadonlyMode<BloggerDbContext>())
            {
                //The operations method doesn't take any dbContext parameter.We are not passing the dbContext around
                return _userOperations.GetUser(userId);
            }
        }

        public async Task<VoUser> GetUserAsync(long userId)
        {
            //We are creating a dbContextScope here which in turn creates an AmbientDbContext
            using (var dbContextScope = DbContextScopeFactory.CreateAmbientDbContextinReadonlyMode<BloggerDbContext>())
            {
                //The interesting thing to notice in the below code is that it doesn't run on the main thread. Instead it spins
                //off its own thread and run the operation on the new thread. Even when you move between threads the 
                //dbContext should/would be available.
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
