using System.Threading.Tasks;
using TestExamples.ValueObjects;

namespace TestExamples.Interfaces
{
    internal interface IUserServices
    {
        VoUser GetUser(long userId);

        Task<VoUser> GetUserAsync(long userId);

        long AddUser(VoUser user);
    }
}
