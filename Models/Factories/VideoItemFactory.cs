using System;
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
            return new VideoItem(poco, this);
        }

        public async Task<IVideoItem> GetVideoItemDbAsync(string id)
        {
            var fb = _c.CreateSqLiteDatabase();

            // var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var fbres = await fb.GetVideoItemAsync(id);
                return new VideoItem(fbres, this);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IVideoItem> GetVideoItemNetAsync(string id)
        {
            var fb = _c.CreateYouTubeSite();
            try
            {
                var fbres = await fb.GetVideoItemNetAsync(id);
                return new VideoItem(fbres, this);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IVideoItem> GetVideoItemLiteNetAsync(string id)
        {
            var fb = _c.CreateYouTubeSite();
            try
            {
                var fbres = await fb.GetVideoItemLiteNetAsync(id);
                return new VideoItem(fbres, this);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IChannel> GetParentChannelAsync(string channelID)
        {
            var fb = _c.CreateSqLiteDatabase();
            try
            {
                var fbres = await fb.GetChannelAsync(channelID);
                return new Channel(fbres, _c.CreateChannelFactory());
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
    }
}
