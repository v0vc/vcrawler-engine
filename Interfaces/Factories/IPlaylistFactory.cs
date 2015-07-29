using System.Threading.Tasks;
using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.Factories
{
    public interface IPlaylistFactory
    {
        IPlaylist CreatePlaylist();
        IPlaylist CreatePlaylist(IPlaylistPOCO poco);
        Task<IPlaylist> GetPlaylistDbAsync(string id);
        Task<IPlaylist> GetPlaylistNetAsync(string id);
    }
}
