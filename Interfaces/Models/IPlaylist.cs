// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Threading.Tasks;
using Interfaces.Enums;

namespace Interfaces.Models
{
    public interface IPlaylist
    {
        #region Properties

        string ChannelId { get; set; }
        string ID { get; set; }
        IEnumerable<string> PlItems { get; set; }
        SiteType Site { get; set; }
        string SubTitle { get; set; }
        byte[] Thumbnail { get; set; }
        string Title { get; set; }

        #endregion

        #region Methods

        Task DeletePlaylistAsync();

        Task DownloadPlaylist();

        Task<IEnumerable<IVideoItem>> GetPlaylistItemsDbAsync();

        Task<IEnumerable<string>> GetPlaylistItemsIdsListDbAsync();

        Task<IEnumerable<string>> GetPlaylistItemsIdsListNetAsync();

        Task<IEnumerable<IVideoItem>> GetPlaylistItemsNetAsync();

        Task InsertPlaylistAsync();

        Task UpdatePlaylistAsync(string videoId);

        #endregion
    }
}
