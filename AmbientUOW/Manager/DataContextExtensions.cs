using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace AmbientDbContext.Manager
{
    internal static class DataContextExtensions
    {
        public static bool IsContextDirty(this DbContext dbContext)
        {
            return
                ((IObjectContextAdapter) dbContext).ObjectContext.ObjectStateManager.GetObjectStateEntries(
                    EntityState.Added | EntityState.Deleted | EntityState.Modified).Any();
        }
    }
}
