// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.Database;
using DataAPI.POCO;
using DataAPI.Videos;
using Extensions;
using Interfaces.Enums;
using Interfaces.Models;
using Models.BO.Items;

namespace Models.Factories
{
    public class VideoItemFactory
    {
        #region Static and Readonly Fields

        private readonly CommonFactory commonFactory;
        private readonly SqLiteDatabase fb;

        #endregion

        #region Constructors

        public VideoItemFactory(CommonFactory commonFactory)
        {
            this.commonFactory = commonFactory;
            fb = commonFactory.CreateSqLiteDatabase();
        }

        #endregion

        #region Static Methods

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

        #region Methods

        public IVideoItem CreateVideoItem(SiteType site)
        {
            switch (site)
            {
                case SiteType.YouTube:
                    return new YouTubeItem(this) { Site = SiteType.YouTube };
                case SiteType.Tapochek:
                    return new TapochekItem { Site = SiteType.Tapochek };
                case SiteType.RuTracker:
                    return new RuTrackerItem { Site = SiteType.RuTracker };
                default:
                    return null;
            }
        }

        public IVideoItem CreateVideoItem(VideoItemPOCO poco)
        {
            var vi = new YouTubeItem(this)
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
                Site = poco.Site, 
                DurationString = IntTostrTime(poco.Duration), 
                DateTimeAgo = TimeAgo(poco.Timestamp), 
                Subtitles = new ObservableCollection<ISubtitle>()
            };
            return vi;
        }

        public async Task FillDescriptionAsync(IVideoItem videoItem)
        {
            string res = await fb.GetVideoItemDescriptionAsync(videoItem.ID);
            videoItem.Description = res.WordWrap(150);
        }

        public async Task<IVideoItem> GetVideoItemDbAsync(string id)
        {
            // var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                VideoItemPOCO poco = await fb.GetVideoItemAsync(id);
                IVideoItem vi = CreateVideoItem(poco);
                return vi;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IVideoItem> GetVideoItemNetAsync(string id, SiteType site)
        {
            try
            {
                VideoItemPOCO poco = null;
                switch (site)
                {
                    case SiteType.YouTube:
                        poco = await commonFactory.CreateYouTubeSite().GetVideoItemNetAsync(id);
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

        public async Task<IEnumerable<ISubtitle>> GetVideoItemSubtitlesAsync(string id)
        {
            SubtitleFactory cf = commonFactory.CreateSubtitleFactory();
            var res = new List<ISubtitle>();
            IEnumerable<SubtitlePOCO> poco = await YouTubeSite.GetVideoSubtitlesByIdAsync(id);
            res.AddRange(poco.Select(cf.CreateSubtitle));
            if (res.Any())
            {
                return res;
            }
            ISubtitle chap = cf.CreateSubtitle();
            chap.IsEnabled = false;
            chap.Language = "Auto";
            res.Add(chap);
            return res;

        }

        public async Task InsertItemAsync(IVideoItem item)
        {
            try
            {
                await fb.InsertItemAsync(item);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion
    }
}
