using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces.Models
{
    public interface ISubscribe
    {
        ISubscribe GetSubscribe();

        Task<List<IChannel>> GetChannelsListAsync();

        Task<List<ICred>> GetCredListAsync();

        Task<List<string>> GetChannelsIdsListDbAsync();
    }
}
