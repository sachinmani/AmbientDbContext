using System.Data.Entity;
using AmbientDbContext.Interfaces;

namespace AmbientDbContext.Manager
{
    public static class DbContextLocator
    {
        public static T GetDbContext<T>() where T : DbContext, IAmbientDbContext, new()
        {
            var dbContext = DbContextScopeFactory.GetDbContext<T>();
            return dbContext;
        }
    }
}
