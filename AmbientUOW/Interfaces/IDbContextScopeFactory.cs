using System.Data;
using System.Data.Common;
using System.Data.Entity;
using AmbientDbContext.Manager;

namespace AmbientDbContext.Interfaces
{
    public interface IDbContextScopeFactory
    {
        DbContextScope<T> CreateDbContextScope<T>(DbContextOption.Mode mode) where T : DbContext, IAmbientDbContext, new();

        DbContextScope<T> CreateDbContextScope<T>(DbContextOption.Mode mode, IsolationLevel isolationLevel) where T : DbContext, IAmbientDbContext, new();

        DbContextScope<T> CreateDbContextScope<T>(DbContextOption.Mode mode, IsolationLevel isolationLevel, DbTransaction dbTransaction, DbConnection connection) where T : DbContext, IAmbientDbContext, new();
    }
}