using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces.Models
{
    public interface ITag
    {
        string Title { get; set; }
        Task DeleteTagAsync();
        Task InsertTagAsync();
        Task<IEnumerable<IChannel>> GetChannelsByTagAsync();
    }
}
