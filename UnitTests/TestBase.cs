using System.Data.Entity;
using DataModel;

namespace UnitTests
{
    public abstract class TestBase
    {
        protected TestBase()
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<BloggerDbContext>());
        }
    }
}
