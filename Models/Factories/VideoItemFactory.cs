using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Models.BO;

namespace Models.Factories
{
    class VideoItemFactory : IVideoItemFactory
    {
        public IVideoItem CreateVideoItem()
        {
            return new VideoItem();
        }

        public async Task<IVideoItem> GetVideoItemDbAsync(string id)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var fbres = await fb.GetVideoItemAsync(id);
                return new VideoItem(fbres);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IVideoItem> GetVideoItemNetAsync(string id)
        {
            var fb = ServiceLocator.YouTubeSite;
            try
            {
                var fbres = await fb.GetVideoItemNetAsync(id);
                return new VideoItem(fbres);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IChannel> GetParentChannelAsync(string channelID)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var fbres = await fb.GetChannelAsync(channelID);
                return new Channel(fbres);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertItemAsync(IVideoItem item)
        {
            var fb = ServiceLocator.SqLiteDatabase;
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
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.DeleteItemAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdatePlaylistAsync(string plid, string itemid, string channelid)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.UpdatePlaylistAsync(plid, itemid, channelid);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
