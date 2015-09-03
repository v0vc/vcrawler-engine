using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;

namespace Interfaces.Models
{
    public interface IChannel
    {
        string ID { get; set; }
        string Title { get; set; }
        string SubTitle { get; set; }
        byte[] Thumbnail { get; set; }
        string Site { get; set; }
        ObservableCollection<IVideoItem> ChannelItems { get; set; }
        ObservableCollection<IPlaylist> ChannelPlaylists { get; set; }
        ObservableCollection<ITag> ChannelTags { get; set; }
        int CountNew { get; set; }
        bool IsDownloading { get; set; }
        bool IsInWork { get; set; }
        bool IsShowRow { get; set; }
        CookieCollection ChannelCookies { get; set; }
        Task<IEnumerable<IVideoItem>> GetChannelItemsDbAsync();
        Task SyncChannelAsync(string dir, bool isSyncPls);
        Task SyncChannelPlaylistsAsync();
        Task<IEnumerable<IVideoItem>> GetChannelItemsNetAsync(int maxresult);
        Task<IEnumerable<IVideoItem>> GetPopularItemsNetAsync(string regionID, int maxresult);
        Task<IEnumerable<IVideoItem>> SearchItemsNetAsync(string key, string region, int maxresult);
        Task<IEnumerable<IPlaylist>> GetChannelPlaylistsNetAsync();
        Task<IEnumerable<IPlaylist>> GetChannelPlaylistsDbAsync();
        Task<int> GetChannelItemsCountDbAsync();
        Task<int> GetChannelItemsCountNetAsync();
        Task<IEnumerable<string>> GetChannelItemsIdsListNetAsync(int maxresult);
        Task<IEnumerable<string>> GetChannelItemsIdsListDbAsync();
        Task FillChannelItemsDbAsync(string dir);
        Task InsertChannelAsync();
        Task DeleteChannelAsync();
        Task RenameChannelAsync(string newName);
        Task InsertChannelItemsAsync();
        Task<IEnumerable<ITag>> GetChannelTagsAsync();
        Task InsertChannelTagAsync(string tag);
        Task DeleteChannelTagAsync(string tag);
        void AddNewItem(IVideoItem item, bool isNew);
        Task FillChannelCookieNetAsync();
        Task StoreCookiesAsync();
        Task FillChannelCookieDbAsync();
        Task FillChannelDescriptionAsync();
    }
}
