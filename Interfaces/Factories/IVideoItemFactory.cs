using System.Threading.Tasks;
using Interfaces.Models;

namespace Interfaces.Factories
{
    public interface IVideoItemFactory
    {
        IVideoItem CreateVideoItem();
        Task<IVideoItem> GetVideoItemDbAsync(string id);
        Task<IVideoItem> GetVideoItemNetAsync(string id);
    }
}
