﻿using System.Threading.Tasks;
using AmbientDbContext.Manager;
using DataModel;
using TestExamples.Interfaces;
using TestExamples.Operations;
using TestExamples.ValueObjects;

namespace TestExamples.Services
{
    public class UserServices : IUserServices
    {
        private readonly UserOperations _userOperations;
        public UserServices()
        {
            _userOperations = new UserOperations();
        }
        public VoUser GetUser(long userId)
        {
            //Demonstration of creating an AmbientDbContext in readonly mode
            using (var dbContextScope = new DbContextScopeFactory().CreateAmbientDbContextInReadonlyMode<BloggerDbContext>())
            {
                //The operations method doesn't take any dbContext parameter.We are not passing the dbContext around
                var user = _userOperations.GetUser(userId);
                return new VoUser
                {
                    Name = user.Name,
                    Occupation = user.Occupation
                };
            }
        }

        public async Task<VoUser> GetUserAsync(long userId)
        {
            //Demonstration of creating an AmbientDbContext in readonly mode
            using (var dbContextScope = new DbContextScopeFactory().CreateAmbientDbContextInReadonlyMode<BloggerDbContext>())
            {
                //The interesting thing to notice in the below code is that it doesn't run on the main thread. Instead it spins
                //off its own thread and run the operation on the new thread. Even when you move between threads the 
                //dbContext should/would be available.
                var user = await _userOperations.GetUserAsync(userId);
                return new VoUser
                {
                    Name = user.Name,
                    Occupation = user.Occupation
                };
            }
        }

        public long AddUser(VoUser user)
        {
            //Demonstration of creating an AmbientDbContext in transaction mode
            using (var dbContextScope = new DbContextScopeFactory().CreateAmbientDbContextInReadonlyMode<BloggerDbContext>())
            {
                var addedUser = _userOperations.AddUser(user);
                dbContextScope.SaveChanges();
                dbContextScope.Commit();
                return addedUser.UserId;
            }
        }
    }
}
