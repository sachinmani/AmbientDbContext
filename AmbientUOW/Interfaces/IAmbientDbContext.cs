using AmbientDbContext.Manager;

namespace AmbientDbContext.Interfaces
{
    //Extend the DbContext with this interface to enable the DbContextScopeFactory to deal with
    //DbContext option.  
    public interface IAmbientDbContext
    {
        DbContextOption.Mode Mode { get; set; }
    }
}
