using System.Threading.Tasks;
using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.Factories
{
    public interface IChannelFactory
    {
        IChannel CreateChannel();
        IChannel CreateChannel(IChannelPOCO poco);
        Task<IChannel> GetChannelDbAsync(string channelID);
        Task<IChannel> GetChannelNetAsync(string channelID);
        Task<string> GetChannelIdByUserNameNetAsync(string username);
    }
}
