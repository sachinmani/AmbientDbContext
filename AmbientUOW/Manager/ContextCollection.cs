using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AmbientDbContext.Interfaces;

namespace AmbientDbContext.Manager
{
    /// <summary>
    /// Responsible for holding and handling all dbContext associated with DbContextScope or Ambient Scope.
    /// </summary>
    public class ContextCollection
    {
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

        /// <summary>
        /// Responsible for adding newly created dbContext and its transaction in a dictionary with type as a key. Before adding to
        /// the dictionary we override the SavingChanges event in objectContext to prevent developer from calling SaveChanges directly from
        /// DbContext(DbContext.SaveChnages) 
        /// </summary>
        /// <typeparam name="T">{T} type</typeparam>
        /// <param name="dbContext">DbContext</param>
        /// <param name="contextTransaction">transaction associated with the new Dbcontext.</param>
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

        /// <summary>
        /// Retrieve the DbContext of type {T} from the dictionary and return it.
        /// </summary>
        /// <typeparam name="T">DbContext to be retrieved.</typeparam>
        /// <returns>DbContext of type {T}</returns>
        internal T GetDbContextByType<T>() where T : DbContext, new()
        {
            if (_dictionary.ContainsKey(typeof(T)))
            {
                return _dictionary[typeof(T)] as T;
            }
            return default(T);
        }

        /// <summary>
        /// Get all DbContext associated with UnitOfWorkScope or Ambient Scope.
        /// </summary>
        /// <returns></returns>
        internal ICollection GetAllDbContext()
        {
            return new[] {_dictionary.Values};
        }

        /// <summary>
        /// Save all dbContext changes and commit the transaction.
        /// </summary>
        /// <param name="implicitCommit"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task CommitAllAsync(bool implicitCommit, CancellationToken cancellationToken)
        {
            ExceptionDispatchInfo exceptionDispatchInfo = null;
            var exceptionOccured = false;
            Debug.WriteLine("Trying to save details");
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
                    Debug.WriteLine(
                        "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(e);
            }
            catch (DbUpdateException e)
            {
                exceptionOccured = true;
                Debug.WriteLine(e.InnerException);
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(e);
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
                if (exceptionDispatchInfo != null)
                {
                    exceptionDispatchInfo.Throw();
                }
            }
        }

        /// <summary>
        /// Commit all dbContext changes and commit the transaction.
        /// </summary>
        /// <param name="implicitCommit"></param>
        internal void CommitAll(bool implicitCommit)
        {
            ExceptionDispatchInfo exceptionDispatchInfo = null;
            var exceptionOccured = false;
            Debug.WriteLine("Trying to save details");
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
                    Debug.WriteLine(
                        "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(e);
            }
            catch (DbUpdateException e)
            {
                exceptionOccured = true;
                Debug.WriteLine(e.InnerException);
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(e);
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
                if (exceptionDispatchInfo != null)
                {
                    exceptionDispatchInfo.Throw();
                }
            }
        }

        /// <summary>
        /// Dispose the dbContexts and  transaction associated with it.
        /// </summary>
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
