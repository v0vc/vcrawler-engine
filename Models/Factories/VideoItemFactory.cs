// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.POCO;
using DataAPI.Videos;
using Extensions;
using Interfaces.Enums;
using Interfaces.Models;
using Models.BO.Items;

namespace Models.Factories
{
    public static class VideoItemFactory
    {
        #region Static Methods

        public static IVideoItem CreateVideoItem(SiteType site)
        {
            switch (site)
            {
                case SiteType.YouTube:
                    return new YouTubeItem();
                case SiteType.Tapochek:
                    return new TapochekItem();
                case SiteType.RuTracker:
                    return new RuTrackerItem();
                default:
                    return null;
            }
        }

        public static IVideoItem CreateVideoItem(VideoItemPOCO poco, SiteType site, SyncState sstate = SyncState.Notset)
        {
            IVideoItem vi;
            switch (site)
            {
                case SiteType.YouTube:
                    vi = new YouTubeItem
                    {
                        ID = poco.ID,
                        Title = poco.Title,
                        ParentID = poco.ParentID,
                        Description = poco.Description, // .WordWrap(80);
                        ViewCount = poco.ViewCount,
                        Duration = poco.Duration,
                        Comments = poco.Comments,
                        Thumbnail = poco.Thumbnail,
                        Timestamp = poco.Timestamp,
                        LikeCount = poco.LikeCount,
                        DislikeCount = poco.DislikeCount,
                        ViewDiff = poco.ViewDiff,
                        SyncState = sstate == SyncState.Notset ? (SyncState)poco.SyncState : sstate,
                        WatchState = (WatchState)poco.WatchState,
                        DurationString = StringExtensions.IntTostrTime(poco.Duration),
                        DateTimeAgo = StringExtensions.TimeAgo(poco.Timestamp),
                        Subtitles = new ObservableCollection<ISubtitle>()
                    };
                    break;
                case SiteType.Tapochek:
                    vi = new TapochekItem();
                    break;
                case SiteType.RuTracker:
                    vi = new RuTrackerItem();
                    break;
                default:
                    vi = null;
                    break;
            }
            return vi;
        }

        public static async Task<IVideoItem> GetVideoItemNetAsync(string id, SiteType site)
        {
            try
            {
                VideoItemPOCO poco = null;
                switch (site)
                {
                    case SiteType.YouTube:
                        poco = await YouTubeSite.GetVideoItemNetAsync(id).ConfigureAwait(false);
                        break;
                }
                IVideoItem vi = CreateVideoItem(poco, site);
                return vi;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<IEnumerable<ISubtitle>> GetVideoItemSubtitlesAsync(string id)
        {
            var res = new List<ISubtitle>();
            List<SubtitlePOCO> poco = await YouTubeSite.GetVideoSubtitlesByIdAsync(id).ConfigureAwait(false);
            res.AddRange(poco.Select(SubtitleFactory.CreateSubtitle));
            if (res.Any())
            {
                return res;
            }
            ISubtitle chap = SubtitleFactory.CreateSubtitle();
            chap.IsEnabled = false;
            chap.Language = "Auto";
            res.Add(chap);
            return res;
        }

        #endregion
    }
}
