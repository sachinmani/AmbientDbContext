using System.Data;
using System.Data.Common;
using System.Data.Entity;
using AmbientDbContext.Manager;

namespace AmbientDbContext.Interfaces
{
    public interface IDbContextScopeFactory
    {
        DbContextScope<T> CreateAmbientDbContext<T>()
            where T : DbContext, IAmbientDbContext, new();

        DbContextScope<T> CreateAmbientDbContextInReadonlyMode<T>()
            where T : DbContext, IAmbientDbContext, new();

        DbContextScope<T> CreateAmbientDbContextInReadonlyMode<T>(IsolationLevel isolationLevel)
            where T : DbContext, IAmbientDbContext, new();

        DbContextScope<T> CreateAmbientDbContextInTransactionMode<T>()
            where T : DbContext, IAmbientDbContext, new();

        DbContextScope<T> CreateAmbientDbContextInTransactionMode<T>(IsolationLevel isolationLevel)
            where T : DbContext, IAmbientDbContext, new();

        DbContextScope<T> CreateAmbientDbContextWithExternalTransaction<T>(DbTransaction dbTransaction, DbConnection connection)
            where T : DbContext, IAmbientDbContext, new(); 
        
        DbContextScope<T> CreateNonAmbientDbContextInTransactionMode<T>()
            where T : DbContext, IAmbientDbContext, new();

        DbContextScope<T> CreateNonAmbientDbContextInTransactionMode<T>(IsolationLevel isolationLevel)
            where T : DbContext, IAmbientDbContext, new();
    }
}