using System.Data;
using System.Data.Common;
using System.Data.Entity;
using AmbientDbContext.Interfaces;

namespace AmbientDbContext.Manager
{
    public class DbContextScopeFactory
    {
        public static DbContextScope<T> CreateAmbientDbContextinReadonlyMode<T>()
            where T : DbContext, IAmbientDbContext, new()
        {
            return CreateDbContextScope<T>(DbContextOption.Mode.Read, IsolationLevel.Serializable);   
        }

        public static DbContextScope<T> CreateAmbientDbContextinTransactionMode<T>()
            where T : DbContext, IAmbientDbContext, new()
        {
            return CreateDbContextScope<T>(DbContextOption.Mode.Write, IsolationLevel.Serializable);
        }

        public static DbContextScope<T> CreateAmbientDbContextinTransactionMode<T>(IsolationLevel isolationLevel)
            where T : DbContext, IAmbientDbContext, new()
        {
            return CreateDbContextScope<T>(DbContextOption.Mode.Write, isolationLevel, DbContextOption.DbTransactionOption.AmbientMode);
        }

        public static DbContextScope<T> CreateNonAmbientDbContextinTransactionMode<T>()
            where T : DbContext, IAmbientDbContext, new()
        {
            return CreateNonAmbientDbContextinTransactionMode<T>(IsolationLevel.Serializable);
        }

        public static DbContextScope<T> CreateNonAmbientDbContextinTransactionMode<T>(IsolationLevel isolationLevel)
            where T : DbContext, IAmbientDbContext, new()
        {
            return CreateDbContextScope<T>(DbContextOption.Mode.Write, isolationLevel, DbContextOption.DbTransactionOption.NonAmbientMode);
        }

        public static DbContextScope<T> CreateAmbientDbContextWithExternalTransaction<T>(DbTransaction dbTransaction, DbConnection connection)
            where T : DbContext, IAmbientDbContext, new()
        {
            return CreateDbContextScope<T>(DbContextOption.Mode.Write, null, DbContextOption.DbTransactionOption.AmbientMode, dbTransaction, connection);
        }

        private static DbContextScope<T> CreateDbContextScope<T>(DbContextOption.Mode mode, IsolationLevel? isolationLevel) where T : DbContext, IAmbientDbContext, new()
        {
            return CreateDbContextScope<T>(mode, isolationLevel, DbContextOption.DbTransactionOption.AmbientMode, null, null);
        }

        private static DbContextScope<T> CreateDbContextScope<T>(DbContextOption.Mode mode, IsolationLevel? isolationLevel, DbContextOption.DbTransactionOption dbTransactionOption) where T : DbContext, IAmbientDbContext, new()
        {
            return CreateDbContextScope<T>(mode, isolationLevel, dbTransactionOption, null, null);
        }

        private static DbContextScope<T> CreateDbContextScope<T>(DbContextOption.Mode mode, IsolationLevel? isolationLevel, DbContextOption.DbTransactionOption dbTransactionOption, DbTransaction dbTransaction, DbConnection connection) where T : DbContext, IAmbientDbContext, new()
        {
            return new DbContextScope<T>(mode, isolationLevel, dbTransaction, connection, dbTransactionOption);
        }

        internal static T GetDbContext<T>() where T : DbContext, IAmbientDbContext, new()
        {
            return DbContextScope<T>.GetDbContext();
        }

        internal static T GetNonAmbientDbContext<T>(DbContextScope<T> dbContextScope) where T : DbContext, IAmbientDbContext, new()
        {
            return dbContextScope.GetNonAmbientDbContext();
        }
    }
}
