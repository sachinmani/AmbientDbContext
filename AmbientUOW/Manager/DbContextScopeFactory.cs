using System.Data;
using System.Data.Common;
using System.Data.Entity;
using AmbientDbContext.Interfaces;

namespace AmbientDbContext.Manager
{
    public class DbContextScopeFactory : IDbContextScopeFactory
    {
        public DbContextScope<T> CreateAmbientDbContext<T>() where T : DbContext, IAmbientDbContext, new()
        {
            return CreateDbContextScope<T>(DbContextOption.Mode.Write, null); 
        }

        public DbContextScope<T> CreateAmbientDbContextInReadonlyMode<T>()
            where T : DbContext, IAmbientDbContext, new()
        {
            return CreateDbContextScope<T>(DbContextOption.Mode.Read, IsolationLevel.Serializable);   
        }

        public DbContextScope<T> CreateAmbientDbContextInReadonlyMode<T>(IsolationLevel isolationLevel) where T : DbContext, IAmbientDbContext, new()
        {
            return CreateDbContextScope<T>(DbContextOption.Mode.Read, isolationLevel);
        }

        public DbContextScope<T> CreateAmbientDbContextInTransactionMode<T>()
            where T : DbContext, IAmbientDbContext, new()
        {
            return CreateDbContextScope<T>(DbContextOption.Mode.Write, IsolationLevel.Serializable);
        }

        public DbContextScope<T> CreateAmbientDbContextInTransactionMode<T>(IsolationLevel isolationLevel)
            where T : DbContext, IAmbientDbContext, new()
        {
            return CreateDbContextScope<T>(DbContextOption.Mode.Write, isolationLevel);
        }

        public DbContextScope<T> CreateNonAmbientDbContextInTransactionMode<T>()
            where T : DbContext, IAmbientDbContext, new()
        {
            return CreateNonAmbientDbContextInTransactionMode<T>(IsolationLevel.Serializable);
        }

        public DbContextScope<T> CreateNonAmbientDbContextInTransactionMode<T>(IsolationLevel isolationLevel)
            where T : DbContext, IAmbientDbContext, new()
        {
            return CreateNonAmbientDbContextScope<T>(DbContextOption.Mode.Write, isolationLevel);
        }

        public DbContextScope<T> CreateNonAmbientDbContextScope<T>(DbContextOption.Mode mode, 
                                                                   IsolationLevel? isolationLevel) where T : DbContext, IAmbientDbContext, new()
        {
            var dbContextScope = new NonAmbientDbContextScope<T>(mode, isolationLevel, null, null, new AmbientDbContextHouseKeeper());
            dbContextScope.Initialize();
            return dbContextScope;
        } 

        public DbContextScope<T> CreateAmbientDbContextWithExternalTransaction<T>(DbTransaction dbTransaction, DbConnection connection)
            where T : DbContext, IAmbientDbContext, new()
        {
            return CreateDbContextScope<T>(DbContextOption.Mode.Write, null, dbTransaction, connection);
        }

        private static DbContextScope<T> CreateDbContextScope<T>(DbContextOption.Mode mode, IsolationLevel? isolationLevel) where T : DbContext, IAmbientDbContext, new()
        {
            return CreateDbContextScope<T>(mode, isolationLevel, null, null);
        }

        private static DbContextScope<T> CreateDbContextScope<T>(DbContextOption.Mode mode, IsolationLevel? isolationLevel, DbTransaction dbTransaction, DbConnection connection) where T : DbContext, IAmbientDbContext, new()
        {
            var dbContextScope  = new DbContextScope<T>(mode, isolationLevel, dbTransaction, connection);
            dbContextScope.Initialize();
            return dbContextScope;
        }

        internal static T GetDbContext<T>() where T : DbContext, IAmbientDbContext, new()
        {
            return DbContextScope<T>.GetDbContext();
        }
    }
}
