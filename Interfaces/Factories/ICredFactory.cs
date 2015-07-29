using System.Threading.Tasks;
using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.Factories
{
    public interface ICredFactory
    {
        ICred CreateCred();
        ICred CreateCred(ICredPOCO poco);
        Task<ICred> GetCredDbAsync(string site);
    }
}
