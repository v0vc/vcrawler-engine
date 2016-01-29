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

        public static IVideoItem CreateVideoItem(VideoItemPOCO poco)
        {
            var vi = new YouTubeItem
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
                SyncState = (SyncState)poco.SyncState,
                WatchState = (WatchState)poco.WatchState,
                DurationString = IntTostrTime(poco.Duration),
                DateTimeAgo = TimeAgo(poco.Timestamp),
                Subtitles = new ObservableCollection<ISubtitle>()
            };
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
                        poco = await YouTubeSite.GetVideoItemNetAsync(id);
                        break;
                }
                IVideoItem vi = CreateVideoItem(poco);
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
            List<SubtitlePOCO> poco = await YouTubeSite.GetVideoSubtitlesByIdAsync(id);
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

        private static string IntTostrTime(int duration)
        {
            TimeSpan t = TimeSpan.FromSeconds(duration);
            return t.Hours > 0
                ? string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds)
                : string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        private static string TimeAgo(DateTime dt)
        {
            TimeSpan span = DateTime.Now - dt;
            if (span.Days > 365)
            {
                int years = span.Days / 365;
                if (span.Days % 365 != 0)
                {
                    years += 1;
                }
                return string.Format("about {0} {1} ago", years, years == 1 ? "year" : "years");
            }
            if (span.Days > 30)
            {
                int months = span.Days / 30;
                if (span.Days % 31 != 0)
                {
                    months += 1;
                }
                return string.Format("about {0} {1} ago", months, months == 1 ? "month" : "months");
            }
            if (span.Days > 0)
            {
                return string.Format("about {0} {1} ago", span.Days, span.Days == 1 ? "day" : "days");
            }
            if (span.Hours > 0)
            {
                return string.Format("about {0} {1} ago", span.Hours, span.Hours == 1 ? "hour" : "hours");
            }
            if (span.Minutes > 0)
            {
                return string.Format("about {0} {1} ago", span.Minutes, span.Minutes == 1 ? "minute" : "minutes");
            }
            if (span.Seconds > 5)
            {
                return string.Format("about {0} seconds ago", span.Seconds);
            }
            if (span.Seconds <= 5)
            {
                return "just now";
            }
            return string.Empty;
        }

        #endregion
    }
}
