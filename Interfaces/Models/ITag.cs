using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces.Models
{
    public interface ITag
    {
        string Title { get; set; }

        List<IChannel> Channels { get; set; }

        Task DeleteTagAsync();

        Task InsertTagAsync();

        Task<List<IChannel>> GetChannelsByTagAsync();
    }
}
