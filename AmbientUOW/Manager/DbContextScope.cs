using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;
using AmbientDbContext.Interfaces;

namespace AmbientDbContext.Manager
{
    public class DbContextScope<T> : IDisposable where T : DbContext, IAmbientDbContext, new()
    {
        private ContextData _savedContextData;

        private ContextData _currentThreadContextData;

        private readonly DbContextOption.DbTransactionOption _dbTransactionOption;

        private bool _isParentScope;

        private bool _isCompleted;

        private readonly DbContextOption.Mode _mode;

        private readonly DbTransaction _dbTransaction;

        private readonly IsolationLevel? _isolationLevel;

        private readonly DbConnection _sqlConnection;

        private readonly ContextKey _contextKey;

        internal DbContextScope(DbContextOption.Mode mode, 
                                IsolationLevel? isolationLevel, 
                                DbTransaction dbTransaction, 
                                DbConnection sqlConnection,
                                DbContextOption.DbTransactionOption dbTransactionOption)
        {
            _mode = mode;
            _dbTransaction = dbTransaction;
            _sqlConnection = sqlConnection;
            _isolationLevel = isolationLevel;
            _dbTransactionOption = dbTransactionOption;
            _contextKey = new ContextKey();
            Initialize();
        }

        private void Initialize()
        {
            if (_dbTransactionOption == DbContextOption.DbTransactionOption.NonAmbientMode)
            {
                _currentThreadContextData = ContextData.CreateContextData<T>(_mode, 
                                                                             _isolationLevel, 
                                                                             _dbTransaction,
                                                                             _sqlConnection);
            }
            else
            {
                _savedContextData = null;
                var contextData = CallContextContextData.GetContextData();
                if (contextData == null)
                {
                    contextData = ContextData.CreateContextData<T>(_mode, _isolationLevel, _dbTransaction,
                        _sqlConnection);
                    _isParentScope = true;
                }
                else
                {
                    _isParentScope = false;
                    var dbcontext = contextData.GetDbContextByType<T>();
                    if (dbcontext == null)
                    {
                        contextData.CreateNewDbContextIfNotExists<T>(_mode, _isolationLevel, _dbTransaction,
                            _sqlConnection);
                    }
                    else
                    {
                        if (dbcontext.Mode == DbContextOption.Mode.Read && _mode == DbContextOption.Mode.Write)
                        {
                            throw new InvalidOperationException("Cannot nested read/write when parent is in read mode");
                        }
                    }
                }

                _savedContextData = contextData;

                //Check the callcontext to see if there is an existing context data.
                contextData = CallContextContextData.GetContextData();
                //Add the context data back to the CallContext with new context key
                if (contextData == null)
                {
                    CallContextContextData.AddContextData(_contextKey, _savedContextData);
                }
            }
        }

        internal static T GetDbContext()
        {
            var contextData = CallContextContextData.GetContextData();
            return contextData.GetDbContextByType<T>();
        }

        internal T GetNonAmbientDbContext()
        {
            if (_currentThreadContextData == null)
            {
                throw new ObjectNotFoundException("No non ambient dbcontext is available");
            }
            return _currentThreadContextData.GetDbContextByType<T>();
        }

        public void SaveChanges(bool implicitCommit = true)
        {
            if (_dbTransactionOption == DbContextOption.DbTransactionOption.NonAmbientMode)
            {
                if (_currentThreadContextData != null)
                {
                    if (_mode == DbContextOption.Mode.Read)
                    {
                        throw new InvalidOperationException("Cannot save changes on a readonly context");
                    }
                    _currentThreadContextData.Commit(true);
                    _isCompleted = true;
                }
                else
                {
                    throw new ObjectNotFoundException("No non ambient dbcontext found");
                }
            }
            else
            {
                if (_savedContextData.Disposed)
                {
                    //this can only happen when a parallel threads are executing with the main thread and the parallel/main
                    //thread has disposed before the main and/or other parallel has finished execution
                    //This should never happen.Only programming error coul cause this issue.
                    throw new ObjectDisposedException("Object already disposed exception");
                }
                var contextData = CallContextContextData.GetContextData();
                if (contextData != _savedContextData)
                {
                    //Duplicate contextdata has been created by someother thread. Could be result of two thread trying to create contextData at same time.
                    throw new Exception(
                        "Duplicate context seen. Something went terribly wrong in the programming. Please check your code to correct.");
                }

                if (!_isParentScope)
                {
                    _isCompleted = true;
                    return;
                }
                _savedContextData.Commit(implicitCommit);
                _isCompleted = true;
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken, bool implicitCommit = true)
        {
            if (_dbTransactionOption == DbContextOption.DbTransactionOption.NonAmbientMode)
            {
                if (_currentThreadContextData != null)
                {
                    if (_mode == DbContextOption.Mode.Read)
                    {
                        throw new InvalidOperationException("Cannot save changes on a readonly context");
                    }
                    await _currentThreadContextData.CommitAsync(true, cancellationToken);
                    _isCompleted = true;
                }
                else
                {
                    throw new ObjectNotFoundException("No non ambient dbcontext found");
                }
            }
            else
            {
                if (_savedContextData.Disposed)
                {
                    //this can only happen when a parallel threads are executing with the main thread and the parallel/main
                    //thread has disposed before the main and/or other parallel has finished execution
                    //This should never happen.Only programming error coul cause this issue.
                    throw new ObjectDisposedException("Object already disposed exception");
                }
                var contextData = CallContextContextData.GetContextData();
                if (contextData != _savedContextData)
                {
                    //Duplicate contextdata has been created by someother thread. Could be result of two thread trying to create contextData at same time.
                    throw new Exception(
                        "Duplicate context seen. Something went terribly wrong in the programming. Please check your code to correct.");
                }
                if (!_isParentScope)
                {
                    _isCompleted = true;
                    return;
                }
                await _savedContextData.CommitAsync(implicitCommit, cancellationToken);
                _isCompleted = true;
            }
        }

        public void RefreshCacheWithUpdatedEntities(IEnumerable entities)
        {
            foreach (var dbContext in _currentThreadContextData.DbContextCollection.GetAllDbContext())
            {
                var ambientContextData = CallContextContextData.GetContextData();
                if (ambientContextData == null)
                {
                    break;
                }

                foreach (var ambientDbContext in ambientContextData.DbContextCollection.GetAllDbContext())
                {
                    if (ambientContextData.GetType() == dbContext.GetType())
                    {
                        foreach (var entity in entities)
                        {
                            ObjectStateEntry objectStateEntry;
                            //Get the entity state entry of the entity from the dbcontext under which it was modified
                            //and get the EntityKey of it so that this key could be used to load the entity from parent
                            //Ambient dbContext if any exists.
                            if (
                                ((IObjectContextAdapter) dbContext).ObjectContext.ObjectStateManager
                                    .TryGetObjectStateEntry(entity, out objectStateEntry))
                            {
                                var key = objectStateEntry.EntityKey;
                                ObjectStateEntry parentObjectStateEntry;
                                if (
                                    ((IObjectContextAdapter) ambientDbContext).ObjectContext.ObjectStateManager
                                        .TryGetObjectStateEntry(key, out parentObjectStateEntry))
                                {
                                    //Modify it only when the parent context entity state entry is unmodified. Else
                                    //just leave it to the database to resolve the issue.
                                    if (parentObjectStateEntry.State == EntityState.Unchanged)
                                    {
                                        ((IObjectContextAdapter) ambientDbContext).ObjectContext.Refresh(
                                            RefreshMode.StoreWins, entity);
                                    }
                                }
                            }
                        }
                    }
                }
            } 
        }

        public void RefreshCacheWithUpdatedEntitiesAsync(IEnumerable entities, CancellationToken cancellationToken)
        {
            foreach (var dbContext in _currentThreadContextData.DbContextCollection.GetAllDbContext())
            {
                var ambientContextData = CallContextContextData.GetContextData();
                if (ambientContextData == null)
                {
                    break;
                }

                foreach (var ambientDbContext in ambientContextData.DbContextCollection.GetAllDbContext())
                {
                    if (ambientContextData.GetType() == dbContext.GetType())
                    {
                        foreach (var entity in entities)
                        {
                            ObjectStateEntry objectStateEntry;
                            //Get the entity state entry of the entity from the dbcontext under which it was modified
                            //and get the EntityKey of it so that this key could be used to load the entity from parent
                            //Ambient dbContext if any exists.
                            if (
                                ((IObjectContextAdapter)dbContext).ObjectContext.ObjectStateManager
                                    .TryGetObjectStateEntry(entity, out objectStateEntry))
                            {
                                var key = objectStateEntry.EntityKey;
                                ObjectStateEntry parentObjectStateEntry;
                                if (
                                    ((IObjectContextAdapter)ambientDbContext).ObjectContext.ObjectStateManager
                                        .TryGetObjectStateEntry(key, out parentObjectStateEntry))
                                {
                                    //Modify it only when the parent context entity state entry is unmodified. Else
                                    //just leave it to the database to resolve the issue.
                                    if (parentObjectStateEntry.State == EntityState.Unchanged)
                                    {
                                        ((IObjectContextAdapter)ambientDbContext).ObjectContext.RefreshAsync(
                                            RefreshMode.StoreWins, entity, cancellationToken);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_dbTransactionOption == DbContextOption.DbTransactionOption.NonAmbientMode)
            {
                if (_currentThreadContextData != null)
                {
                    if (!_isCompleted && _mode == DbContextOption.Mode.Write)
                    {
                        //Rollback the changes done inside the context since SaveChanges was not called.
                        _currentThreadContextData.Rollback();
                    }
                    _currentThreadContextData.Dispose();
                }
            }
            else
            {
                if (_savedContextData.Disposed)
                {
                    //too late to throw error.
                    return;
                }

                if (!_isParentScope)
                {
                    _savedContextData = null;
                    return;
                }

                var contextData = CallContextContextData.GetContextData();
                if (contextData != _savedContextData)
                {
                    //Duplicate contextdata has been created by someother thread. Could be result of two thread trying to create contextData at same time.
                    throw new Exception(
                        "Context disposed or duplicate context seen. Something went terribly wrong in the programming. Please check your code to correct.");
                }
                if (_isParentScope)
                {
                    if (!_isCompleted && _mode == DbContextOption.Mode.Write)
                    {
                        //Cleaning as much as possible. If DbContext is getting disposed before transaction is completed
                        // atleast rollback all uncommitted transaction.
                        _savedContextData.Rollback();
                    }
                    contextData.Dispose();
                    //Remove the contextData from the CallContext.
                    CallContextContextData.RemoveContextData();
                    contextData.Disposed = true;
                }
            }
        }
    }
}
