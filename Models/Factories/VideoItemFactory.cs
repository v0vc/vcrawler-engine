// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using Interfaces.API;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO;

namespace Models.Factories
{
    public class VideoItemFactory : IVideoItemFactory
    {
        #region Static and Readonly Fields

        private readonly ICommonFactory _c;

        #endregion

        #region Constructors

        public VideoItemFactory(ICommonFactory c)
        {
            _c = c;
        }

        #endregion

        #region Methods

        public async Task DeleteItemAsync(string id)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
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
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            string res = await fb.GetVideoItemDescriptionAsync(videoItem.ID);
            videoItem.Description = res.WordWrap(150);
        }

        public async Task<IChannel> GetParentChannelAsync(string channelID)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            IChannelFactory cf = _c.CreateChannelFactory();
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

        public async Task<IEnumerable<IChapter>> GetVideoItemChaptersAsync(string id)
        {
            IYouTubeSite fb = _c.CreateYouTubeSite();
            var res = new List<IChapter>();
            try
            {
                IChapterFactory cf = _c.CreateChapterFactory();
                IEnumerable<IChapterPOCO> poco = await fb.GetVideoSubtitlesByIdAsync(id);
                res.AddRange(poco.Select(cf.CreateChapter));
                if (res.Any())
                {
                    return res;
                }
                IChapter chap = cf.CreateChapter();
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

        public async Task<IVideoItem> GetVideoItemLiteNetAsync(string id)
        {
            IYouTubeSite fb = _c.CreateYouTubeSite();
            IVideoItemFactory vf = _c.CreateVideoItemFactory();
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

        public async Task InsertItemAsync(IVideoItem item)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
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

        public IVideoItem CreateVideoItem()
        {
            return new VideoItem(this);
        }

        public IVideoItem CreateVideoItem(IVideoItemPOCO poco)
        {
            var vi = new VideoItem(this)
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
                VideoItemChapters = new ObservableCollection<IChapter>()
            };
            return vi;
        }

        public async Task<IVideoItem> GetVideoItemDbAsync(string id)
        {
            // var fb = ServiceLocator.SqLiteDatabase;
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            IVideoItemFactory vf = _c.CreateVideoItemFactory();

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

        public async Task<IVideoItem> GetVideoItemNetAsync(string id)
        {
            IYouTubeSite fb = _c.CreateYouTubeSite();
            IVideoItemFactory vf = _c.CreateVideoItemFactory();
            try
            {
                IVideoItemPOCO poco = await fb.GetVideoItemNetAsync(id);
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
