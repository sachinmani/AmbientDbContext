using System.Data;
using System.Data.Common;
using System.Data.Entity;
using AmbientDbContext.Manager;

namespace AmbientDbContext.Interfaces
{
    /// <summary>
    /// Factory responsible for creating different types/modes of unit of work scopes.
    /// </summary>
    public interface IDbContextScopeFactory
    {
        /// <summary>
        /// Creates an ambient readonly dbContextScope in transaction mode with default isolationLevel(IsolationLevel.ReadCommitted)   
        /// </summary>
        DbContextScope<T> CreateAmbientDbContextInReadonlyMode<T>()
            where T : DbContext, IAmbientDbContext, new();

        /// <summary>
        /// Creates an ambient readonly dbContextScope with the given isolation level in transaction mode. When
        /// nesting the dbContextScope, overriding the isolation level in the child scope will throw exception.       
        /// </summary>
        DbContextScope<T> CreateAmbientDbContextInReadonlyMode<T>(IsolationLevel isolationLevel)
            where T : DbContext, IAmbientDbContext, new();
        
        /// <summary>
        /// Creates an ambient dbContextScope in transaction mode by default ReadCommitted isolation level.
        /// </summary>
        /// <typeparam name="T">Type of DbContext</typeparam>
        DbContextScope<T> CreateAmbientDbContextInTransactionMode<T>()
            where T : DbContext, IAmbientDbContext, new();
        
        /// <summary>
        /// Creates an ambient dbContextScope in transaction mode with the given isolation level. When
        /// nesting the dbContextScope, overriding the isolation level in the child scope will throw exception.
        /// </summary>
        DbContextScope<T> CreateAmbientDbContextInTransactionMode<T>(IsolationLevel isolationLevel)
            where T : DbContext, IAmbientDbContext, new();
        
        /// <summary>
        /// Creates an ambient dbContextScope with external Dbtransaction and existing database connection.
        /// </summary>
        DbContextScope<T> CreateAmbientDbContextWithExternalTransaction<T>(DbTransaction dbTransaction, DbConnection connection)
            where T : DbContext, IAmbientDbContext, new(); 
        
        /// <summary>
        /// Creates an standalone dbcontext in the transaction mode. StandAlone DbContextScope cannot/should not be nested.
        /// </summary>
        DbContextScope<T> CreateNonAmbientDbContextInTransactionMode<T>()
            where T : DbContext, IAmbientDbContext, new();

        /// <summary>
        /// Creates an standalone dbcontext with the given isolatonLevel in the transaction mode. StandAlone DbContextScope cannot/should not be nested.
        /// </summary>
        DbContextScope<T> CreateNonAmbientDbContextInTransactionMode<T>(IsolationLevel isolationLevel)
            where T : DbContext, IAmbientDbContext, new();
    }
}