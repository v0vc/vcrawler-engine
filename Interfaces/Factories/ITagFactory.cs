using System.Collections.Generic;
using System.Threading.Tasks;
using Interfaces.Models;

namespace Interfaces.Factories
{
    public interface ITagFactory
    {
        ITag CreateTag();
        Task DeleteTagAsync(string tag);
        Task InsertTagAsync(ITag tag);
        Task<List<IChannel>> GetChannelsByTagAsync(string tag);
    }
}
