using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Models
{
    public interface IVideoItem
    {
        string ID { get; set; }

        string ParentID { get; set; }

        string Title { get; set; }

        string Description { get; set; }

        int ViewCount { get; set; }

        int Duration { get; set; }

        string DurationString { get; set; }

        int Comments { get; set; }

        byte[] Thumbnail { get; set; }

        DateTime Timestamp { get; set; }

        IVideoItem CreateVideoItem();

        Task<IVideoItem> GetVideoItemDbAsync();

        Task<IVideoItem> GetVideoItemNetAsync();

        Task<IChannel> GetParentChannelAsync();

        Task InsertItemAsync();

        Task DeleteItemAsync();

        Task UpdatePlaylistAsync(IPlaylist playlist);
    }
}
