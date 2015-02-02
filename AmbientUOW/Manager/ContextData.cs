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
    internal class ContextData : IDisposable
    {
        internal ContextCollection DbContextCollection { get; private set; }

        internal bool Disposed { get; set; }

        internal bool AllowCommit { get; set; }

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

        internal void Commit(bool implicitCommit)
        {
            DbContextCollection.CommitAll(implicitCommit);
        }

        internal async Task CommitAsync(bool implicitCommit, CancellationToken cancellationToken)
        {
            await DbContextCollection.CommitAllAsync(implicitCommit, cancellationToken);
        }

        public void Rollback()
        {
            DbContextCollection.Rollback();
        }

        public void Dispose()
        {
            DbContextCollection.Dispose();
        }
    }
}
