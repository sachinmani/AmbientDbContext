using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AmbientDbContext.Interfaces;

namespace AmbientDbContext.Manager
{
    /// <summary>
    /// Responsible for creating, handling and storing all DbContexts and its data associated with DbContextScope or Ambient Scope. 
    /// </summary>
    internal class ContextData : IDisposable
    {
        internal ContextCollection DbContextCollection { get; private set; }

        internal bool Disposed { get; set; }

        internal bool AllowCommit { get; set; }

        internal IsolationLevel IsolationLevel { get; set; }

        internal ContextData()
        {
            DbContextCollection = new ContextCollection();
        }

        internal static ContextData CreateContextData<T>(DbContextOption.Mode mode, IsolationLevel isolationLevel) where T : DbContext, IAmbientDbContext, new()
        {
            return CreateContextData<T>(mode, isolationLevel, null, null);
        }

        internal static ContextData CreateContextData<T>(DbContextOption.Mode mode, IsolationLevel? isolationLevel, DbTransaction transaction, DbConnection sqlConnection) where T : DbContext, IAmbientDbContext, new()
        {
            Debug.WriteLine("Trying to create context data");
            var contextData = new ContextData();
            contextData.CreateNewDbContextIfNotExists<T>(mode, isolationLevel, transaction, sqlConnection);
            return contextData;
        }

        internal T GetDbContextByType<T>() where T : DbContext, IAmbientDbContext, new()
        {
            return DbContextCollection.GetDbContextByType<T>();
        }

        /// <summary>
        /// Responsible for creating a new dbContext with given <see cref="DbContextOption.Mode"/>
        /// </summary>
        /// <typeparam name="T">dbContext of type {T} to be created.</typeparam>
        /// <param name="mode"><see cref="DbContextOption.Mode"/> to create dbContext with.</param>
        /// <param name="isolationLevel"><see cref="IsolationLevel"/> to create dbContext with.</param>
        /// <param name="dbTransaction">create a dbContext with external transaction</param>
        /// <param name="sqlConnection">create a dbContext with existing sql connection</param>
        /// <returns>true if new dbContext created else false.</returns>
        internal bool CreateNewDbContextIfNotExists<T>(DbContextOption.Mode mode, IsolationLevel? isolationLevel, DbTransaction dbTransaction, DbConnection sqlConnection) where T : DbContext, IAmbientDbContext, new()
        {
            if (DbContextCollection.GetDbContextByType<T>() != null) return false;
            //Creating a new DbContext Type
            T currentDbContext;
            if (dbTransaction != null && sqlConnection != null)
            {
                currentDbContext = Activator.CreateInstance(typeof (T), sqlConnection, false) as T;
            }
            else
            {
                currentDbContext = Activator.CreateInstance(typeof(T)) as T;
            }
            
            if (currentDbContext != null)
            {
                currentDbContext.Mode = mode;

                if (mode == DbContextOption.Mode.Read)
                {
                    currentDbContext.Configuration.AutoDetectChangesEnabled = false;
                }
                if (dbTransaction != null)
                {
                    currentDbContext.Database.UseTransaction(dbTransaction);
                    DbContextCollection.Add(currentDbContext, null);
                }
                else
                {
                    if (isolationLevel.HasValue)
                    {
                        IsolationLevel = isolationLevel.Value;
                        var transaction = currentDbContext.Database.BeginTransaction(isolationLevel.Value);
                        DbContextCollection.Add(currentDbContext, transaction);
                    }
                    else
                    {
                        var transaction = currentDbContext.Database.BeginTransaction();
                        DbContextCollection.Add(currentDbContext, transaction);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Save all dirty object in the dbcontext but the transaction is not committed yet.
        /// </summary>
        internal void Save()
        {
            DbContextCollection.SaveChanges();
        }

        /// <summary>
        /// Save all dirty object in the dbcontext but the transaction is not committed yet.
        /// </summary>
        internal Task SaveAsync(CancellationToken cancellationToken)
        {
            return DbContextCollection.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Commit all transaction.
        /// </summary>
        internal void Commit()
        {
            DbContextCollection.Commit();
        }

        /// <summary>
        /// Commit all dbContext transactions.
        /// </summary>
        internal void SaveAndCommit()
        {
            DbContextCollection.SaveAndCommitChanges();
        }

        /// <summary>
        /// Commit all dbContext transactions.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        internal async Task SaveAndCommitAsync(CancellationToken cancellationToken)
        {
            await DbContextCollection.SaveAndCommitChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Rollback all dbContext transactions.
        /// </summary>
        public void Rollback()
        {
            DbContextCollection.Rollback();
        }

        /// <summary>
        /// Dispose all dbContexts.
        /// </summary>
        public void Dispose()
        {
            DbContextCollection.Dispose();
        }
    }
}
