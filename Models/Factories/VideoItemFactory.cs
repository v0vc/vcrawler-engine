using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO;

namespace Models.Factories
{
    public class VideoItemFactory : IVideoItemFactory
    {
        private readonly ICommonFactory _c;

        public VideoItemFactory(ICommonFactory c)
        {
            _c = c;
        }

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
                VideoItemChapters = new ObservableCollection<IChapter>()
            };
            return vi;
        }

        public async Task<IVideoItem> GetVideoItemDbAsync(string id)
        {
            // var fb = ServiceLocator.SqLiteDatabase;
            var fb = _c.CreateSqLiteDatabase();
            var vf = _c.CreateVideoItemFactory();
            
            try
            {
                var poco = await fb.GetVideoItemAsync(id);
                var vi = vf.CreateVideoItem(poco);
                return vi;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IVideoItem> GetVideoItemNetAsync(string id)
        {
            var fb = _c.CreateYouTubeSite();
            var vf = _c.CreateVideoItemFactory();
            try
            {
                var poco = await fb.GetVideoItemNetAsync(id);
                var vi = vf.CreateVideoItem(poco);
                return vi;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IVideoItem> GetVideoItemLiteNetAsync(string id)
        {
            var fb = _c.CreateYouTubeSite();
            var vf = _c.CreateVideoItemFactory();
            try
            {
                var poco = await fb.GetVideoItemLiteNetAsync(id);
                var vi = vf.CreateVideoItem(poco);
                return vi;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IChannel> GetParentChannelAsync(string channelID)
        {
            var fb = _c.CreateSqLiteDatabase();
            var cf = _c.CreateChannelFactory();
            try
            {
                var poco = await fb.GetChannelAsync(channelID);
                var channel = cf.CreateChannel(poco);
                return channel;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertItemAsync(IVideoItem item)
        {
            var fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.InsertItemAsync(item);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteItemAsync(string id)
        {
            var fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.DeleteItemAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IChapter>> GetVideoItemChaptersAsync(string id)
        {
            var fb = _c.CreateYouTubeSite();
            var res = new List<IChapter>();
            try
            {
                var cf = _c.CreateChapterFactory();
                var poco = await fb.GetVideoSubtitlesByIdAsync(id);
                res.AddRange(poco.Select(chapterPoco => cf.CreateChapter(chapterPoco)));
                if (!res.Any())
                {
                    var chap = cf.CreateChapter();
                    chap.IsEnabled = false;
                    chap.Language = "Auto";
                    res.Add(chap);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
