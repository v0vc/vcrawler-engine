using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Models;


namespace Interfaces.Factories
{
    public interface IChannelFactory
    {
        IChannel CreateChannel();

        Task<IChannel> GetChannelDbAsync(string channelID);

        Task<IChannel> GetChannelNetAsync(string channelID);
    }
}
