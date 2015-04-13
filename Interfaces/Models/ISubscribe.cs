using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces.Models
{
    public interface ISubscribe
    {
        ISubscribe GetSubscribe();

        Task<List<IChannel>> GetChannelsListAsync();
    }
}
