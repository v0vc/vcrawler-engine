using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces.Models
{
    public interface IPlaylist
    {
        string ID { get; set; }
        string Title { get; set; }
        string SubTitle { get; set; }
        byte[] Thumbnail { get; set; }
        string ChannelId { get; set; }
        List<IVideoItem> PlaylistItems { get; set; }
        Task DeletePlaylistAsync();
        Task InsertPlaylistAsync();
        Task<List<IVideoItem>> GetPlaylistItemsNetAsync();
        Task<List<string>> GetPlaylistItemsIdsListNetAsync();
        Task<List<string>> GetPlaylistItemsIdsListDbAsync();
        Task<List<IVideoItem>> GetPlaylistItemsDbAsync();
        Task UpdatePlaylistAsync(string videoId);
    }
}
