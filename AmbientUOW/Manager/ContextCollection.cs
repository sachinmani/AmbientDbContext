using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Threading;
using System.Threading.Tasks;
using AmbientDbContext.Interfaces;
using NLog;

namespace AmbientDbContext.Manager
{
    public class ContextCollection
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IDictionary<Type, object> _dictionary;
        private readonly IDictionary<Type, DbContextTransaction> _transactionDictionary;
        private bool _allowSaving;
        internal ContextCollection()
        {
            _allowSaving = false;
            _dictionary = new Dictionary<Type, object>();
            _transactionDictionary = new Dictionary<Type, DbContextTransaction>();
        }

        internal int Count
        {
            get { return _dictionary.Count; }
        }

        internal void Add<T>(T dbContext, DbContextTransaction contextTransaction) where T : DbContext, IAmbientDbContext, new()
        {
            //Prevent developers from calling DbContext.SaveChanges directly
            ((IObjectContextAdapter)dbContext).ObjectContext.SavingChanges += GuardAgainstDirectSaves;

            _dictionary.Add(typeof(T), dbContext);
            if (contextTransaction != null)
            {
                _transactionDictionary.Add(typeof(T), contextTransaction);
            }
        }

        private void GuardAgainstDirectSaves(object sender, EventArgs eventArgs)
        {
            if (!_allowSaving)
            {
                throw new InvalidOperationException("Cannot call save from DbContext.");
            }
        }

        internal T GetDbContextByType<T>() where T : DbContext, new()
        {
            if (_dictionary.ContainsKey(typeof(T)))
            {
                return _dictionary[typeof(T)] as T;
            }
            return default(T);
        }

        internal ICollection GetAllDbContext()
        {
            return new[] {_dictionary.Values};
        } 

        internal async Task CommitAllAsync(bool implicitCommit, CancellationToken cancellationToken)
        {
            var exceptionOccured = false;
            Log.Debug("Trying to save details");
            try
            {
                foreach (var dbContext in _dictionary.Values)
                {
                    var context = ((IAmbientDbContext) dbContext);
                    if (context.Mode == DbContextOption.Mode.Read && ((DbContext) dbContext).IsContextDirty())
                    {
                        throw new InvalidOperationException("Cannot modify entities on a readonly context");
                    }
                    _allowSaving = true;
                    await ((DbContext)dbContext).SaveChangesAsync(cancellationToken);
                }
            }
            catch (DbEntityValidationException e)
            {
                exceptionOccured = true;
                foreach (var eve in e.EntityValidationErrors)
                {
                    Log.Error(
                        "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Log.Error("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            finally
            {
                foreach (var contextTransaction in _transactionDictionary.Values)
                {
                    if (implicitCommit && !exceptionOccured)
                    {
                        contextTransaction.Commit();
                    }
                    else
                    {
                        contextTransaction.Rollback();
                    }
                }

            }
        }

        internal void CommitAll(bool implicitCommit)
        {
            var exceptionOccured = false;
            Log.Debug("Trying to save details");
            try
            {
                foreach (var dbContext in _dictionary.Values)
                {
                    var context = ((IAmbientDbContext)dbContext);
                    if (context.Mode == DbContextOption.Mode.Read && ((DbContext)dbContext).IsContextDirty())
                    {
                        throw new InvalidOperationException("Cannot modify entities on a readonly context");
                    }
                    _allowSaving = true;
                    ((DbContext)dbContext).SaveChanges();
                }
            }
            catch (DbEntityValidationException e)
            {
                exceptionOccured = true;
                foreach (var eve in e.EntityValidationErrors)
                {
                    Log.Error(
                        "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Log.Error("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            finally
            {
                foreach (var contextTransaction in _transactionDictionary.Values)
                {
                    if (implicitCommit && !exceptionOccured)
                    {
                        contextTransaction.Commit();
                    }
                    else
                    {
                        contextTransaction.Rollback();
                    }
                }
            }
        }

        internal void Dispose()
        {
            foreach (var dbcontext in _dictionary)
            {
                if (_transactionDictionary.ContainsKey(dbcontext.Key))
                {
                    var contextTransaction = _transactionDictionary[dbcontext.Key];
                    contextTransaction.Dispose();
                }
                ((IObjectContextAdapter)dbcontext.Value).ObjectContext.SavingChanges -= GuardAgainstDirectSaves;
                ((DbContext)dbcontext.Value).Dispose();
            }
            _dictionary.Clear();
        }

        internal void Rollback()
        {
            foreach (var dbcontext in _dictionary)
            {
                if (_transactionDictionary.ContainsKey(dbcontext.Key))
                {
                    var contextTransaction = _transactionDictionary[dbcontext.Key];
                    contextTransaction.Rollback();
                }
            }
        }
    }
}
