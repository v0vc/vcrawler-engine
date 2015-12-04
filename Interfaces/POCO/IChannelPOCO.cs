// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;

namespace Interfaces.POCO
{
    public interface IChannelPOCO
    {
        #region Properties

        string ID { get; }
        string Site { get; }
        string SubTitle { get; }
        byte[] Thumbnail { get; }
        string Title { get; }

        List<IPlaylistPOCO> Playlists { get; set; }
        List<IVideoItemPOCO> Items { get; set; }
        #endregion
    }
}
