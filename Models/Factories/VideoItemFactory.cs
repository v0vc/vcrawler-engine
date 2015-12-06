// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using Interfaces.API;
using Interfaces.Enums;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO.Items;

namespace Models.Factories
{
    public class VideoItemFactory : IVideoItemFactory
    {
        #region Static and Readonly Fields

        private readonly CommonFactory commonFactory;

        #endregion

        #region Constructors

        public VideoItemFactory(CommonFactory commonFactory)
        {
            this.commonFactory = commonFactory;
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

        public async Task DeleteItemAsync(string id)
        {
            ISqLiteDatabase fb = commonFactory.CreateSqLiteDatabase();
            try
            {
                await fb.DeleteItemAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task FillDescriptionAsync(IVideoItem videoItem)
        {
            ISqLiteDatabase fb = commonFactory.CreateSqLiteDatabase();
            string res = await fb.GetVideoItemDescriptionAsync(videoItem.ID);
            videoItem.Description = res.WordWrap(150);
        }

        public async Task<IChannel> GetParentChannelAsync(string channelID)
        {
            ISqLiteDatabase fb = commonFactory.CreateSqLiteDatabase();
            IChannelFactory cf = commonFactory.CreateChannelFactory();
            try
            {
                IChannelPOCO poco = await fb.GetChannelAsync(channelID);
                IChannel channel = cf.CreateChannel(poco);
                return channel;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IVideoItem> GetVideoItemLiteNetAsync(string id)
        {
            IYouTubeSite fb = commonFactory.CreateYouTubeSite();
            IVideoItemFactory vf = commonFactory.CreateVideoItemFactory();
            try
            {
                IVideoItemPOCO poco = await fb.GetVideoItemLiteNetAsync(id);
                IVideoItem vi = vf.CreateVideoItem(poco);
                return vi;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<ISubtitle>> GetVideoItemSubtitlesAsync(string id)
        {
            IYouTubeSite fb = commonFactory.CreateYouTubeSite();
            ISubtitleFactory cf = commonFactory.CreateSubtitleFactory();
            var res = new List<ISubtitle>();
            try
            {
                IEnumerable<ISubtitlePOCO> poco = await fb.GetVideoSubtitlesByIdAsync(id);
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertItemAsync(IVideoItem item)
        {
            ISqLiteDatabase fb = commonFactory.CreateSqLiteDatabase();
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

        #region IVideoItemFactory Members

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

        public IVideoItem CreateVideoItem(IVideoItemPOCO poco)
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
                Site = poco.Site,
                DurationString = IntTostrTime(poco.Duration),
                DateTimeAgo = TimeAgo(poco.Timestamp),
                Subtitles = new ObservableCollection<ISubtitle>()
            };
            return vi;
        }

        public async Task<IVideoItem> GetVideoItemDbAsync(string id)
        {
            // var fb = ServiceLocator.SqLiteDatabase;
            ISqLiteDatabase fb = commonFactory.CreateSqLiteDatabase();
            IVideoItemFactory vf = commonFactory.CreateVideoItemFactory();

            try
            {
                IVideoItemPOCO poco = await fb.GetVideoItemAsync(id);
                IVideoItem vi = vf.CreateVideoItem(poco);
                return vi;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IVideoItem> GetVideoItemNetAsync(string id, SiteType site)
        {
            IVideoItemFactory vf = commonFactory.CreateVideoItemFactory();
            try
            {
                IVideoItemPOCO poco = null;
                switch (site)
                {
                    case SiteType.YouTube:
                        poco = await commonFactory.CreateYouTubeSite().GetVideoItemNetAsync(id);
                        break;
                }
                IVideoItem vi = vf.CreateVideoItem(poco);
                return vi;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion
    }
}
