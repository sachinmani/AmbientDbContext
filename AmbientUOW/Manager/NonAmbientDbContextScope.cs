using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using AmbientDbContext.Interfaces;

namespace AmbientDbContext.Manager
{
    /// <summary>
    /// Responsible for creating a non ambient dbcontext. Memento pattern is used here to copy the ambient dbcontext
    /// and create non ambient dbcontext. After successfully disposing the non ambient dbcontext the ambient dbcontext
    /// is restored.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NonAmbientDbContextScope<T> : DbContextScope<T>, IDisposable where T : DbContext, IAmbientDbContext, new()
    {
        private readonly AmbientDbContextHouseKeeper _ambientDbContextHouseKeeper;

        internal NonAmbientDbContextScope(DbContextOption.Mode mode,
                                          IsolationLevel? isolationLevel,
                                          DbTransaction dbTransaction,
                                          DbConnection sqlConnection,
                                          AmbientDbContextHouseKeeper ambientDbContextHouseKeeper)
            : base(mode, isolationLevel, dbTransaction, sqlConnection)
        {
            _ambientDbContextHouseKeeper = ambientDbContextHouseKeeper;
        }

        internal override void Initialize()
        {
            //copy the ambient dbcontext to the memento object
            var contextData = CallContextContextData.GetContextData();
            _ambientDbContextHouseKeeper.Memento = new Memento
            {
                ContextData = contextData
            };
            CallContextContextData.RemoveContextData();
            base.Initialize();
        }

        public new void Dispose()
        {
            base.Dispose();
            var memento = _ambientDbContextHouseKeeper.Memento;
            CallContextContextData.AddContextData(ContextKey, memento.ContextData);
        }
    }
}
