// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Data;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class PlaylistPOCO : IPlaylistPOCO
    {
        #region Constructors

        public PlaylistPOCO(IDataRecord reader)
        {
            ID = reader[SqLiteDatabase.PlaylistID] as string;
            Title = reader[SqLiteDatabase.PlaylistTitle] as string;
            SubTitle = reader[SqLiteDatabase.PlaylistSubTitle] as string;
            Thumbnail = reader[SqLiteDatabase.PlaylistThumbnail] as byte[];
            ChannelID = reader[SqLiteDatabase.PlaylistChannelId] as string;
        }

        #endregion

        #region IPlaylistPOCO Members

        public string ChannelID { get; private set; }
        public string ID { get; private set; }
        public string SubTitle { get; private set; }
        public byte[] Thumbnail { get; private set; }
        public string Title { get; private set; }

        #endregion
    }
}
