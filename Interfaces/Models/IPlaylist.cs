using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Models
{
    public interface IPlaylist
    {
        string ID { get; set; }

        string Title { get; set; }

        string SubTitle { get; set; }

        string Link { get; set; }

        string ChannelId { get; set;}

        List<IVideoItem> PlaylistItems { get; set; }

        Task DeletePlaylistAsync();

        Task InsertPlaylistAsync();

        Task<List<IVideoItem>> GetPlaylistItemsNetAsync();

        Task<List<IVideoItem>> GetPlaylistItemsDbAsync();
    }
}
