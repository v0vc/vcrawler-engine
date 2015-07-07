using System.Threading.Tasks;
using Interfaces.Models;

namespace Interfaces.Factories
{
    public interface IPlaylistFactory
    {
        IPlaylist CreatePlaylist();
        Task<IPlaylist> GetPlaylistDbAsync(string id);
        Task<IPlaylist> GetPlaylistNetAsync(string id);
    }
}
