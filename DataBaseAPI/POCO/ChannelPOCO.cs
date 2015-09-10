// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Data;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class ChannelPOCO : IChannelPOCO
    {
        #region Constructors

        public ChannelPOCO(IDataRecord reader)
        {
            ID = reader[SqLiteDatabase.ChannelId] as string;
            Title = reader[SqLiteDatabase.ChannelTitle] as string;

            // SubTitle = reader[SqLiteDatabase.ChannelSubTitle] as string;
            Thumbnail = reader[SqLiteDatabase.ChannelThumbnail] as byte[];
            Site = reader[SqLiteDatabase.ChannelSite] as string;
        }

        #endregion

        #region Properties

        public DateTime LastUpdated { get; set; }

        #endregion

        #region IChannelPOCO Members

        public string ID { get; private set; }
        public string Site { get; private set; }
        public string SubTitle { get; private set; }
        public byte[] Thumbnail { get; private set; }
        public string Title { get; private set; }

        #endregion
    }
}
