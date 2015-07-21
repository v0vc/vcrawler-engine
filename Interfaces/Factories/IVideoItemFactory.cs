using System.Threading.Tasks;
using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.Factories
{
    public interface IVideoItemFactory
    {
        IVideoItem CreateVideoItem();
        IVideoItem CreateVideoItem(IVideoItemPOCO poco);
        Task<IVideoItem> GetVideoItemDbAsync(string id);
        Task<IVideoItem> GetVideoItemNetAsync(string id);
    }
}
