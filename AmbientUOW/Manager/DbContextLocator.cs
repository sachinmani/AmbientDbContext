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

        public static T GetNonAmbientDbContext<T>(this DbContextScope<T> contextScope) where T : DbContext, IAmbientDbContext, new()
        {
            var dbContext = DbContextScopeFactory.GetNonAmbientDbContext(contextScope);
            return dbContext;
        }
    }
}
