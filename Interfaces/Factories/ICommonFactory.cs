// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using Interfaces.API;
using Interfaces.Models;

namespace Interfaces.Factories
{
    public interface ICommonFactory
    {
        #region Methods

        IChannelFactory CreateChannelFactory();

        IChapterFactory CreateChapterFactory();

        ICredFactory CreateCredFactory();

        IPlaylistFactory CreatePlaylistFactory();

        ISettingFactory CreateSettingFactory();

        ISqLiteDatabase CreateSqLiteDatabase();

        ITagFactory CreateTagFactory();

        ITapochekSite CreateTapochekSite();

        IVideoItemFactory CreateVideoItemFactory();

        IYouTubeSite CreateYouTubeSite();

        #endregion
    }
}
