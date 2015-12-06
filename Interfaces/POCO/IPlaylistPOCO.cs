// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;

namespace Interfaces.POCO
{
    public interface IPlaylistPOCO
    {
        #region Properties

        string ChannelID { get; }
        string ID { get; }
        string SubTitle { get; set; }
        byte[] Thumbnail { get; }
        string Title { get; }

        List<IVideoItemPOCO> PlaylistItems { get; set; }
        #endregion
    }
}
