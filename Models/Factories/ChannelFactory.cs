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
    internal class ChannelFactory : IChannelFactory
    {
        public IChannel CreateChannel()
        {
            return new Channel();
        }

        public async Task<IChannel> GetChannelDbAsync(string channelID)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var poco = await fb.GetChannelAsync(channelID);
                return new Channel(poco);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IChannel> GetChannelNetAsync(string channelID)
        {
            var fb = ServiceLocator.YouTubeSite;
            try
            {
                var poco = await fb.GetChannelNetAsync(channelID);
                return new Channel(poco);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IVideoItem>> GetChannelItemsAsync(string parentID)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.GetChannelItemsAsync(parentID);
                lst.AddRange(fbres.Select(item => new VideoItem(item)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertChannelAsync(IChannel channel)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.InsertChannelAsync(channel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteChannelAsync(string parentID)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.DeleteChannelAsync(parentID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task RenameChannelAsync(string parentID, string newName)
        {

            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.RenameChannelAsync(parentID, newName);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        public async Task InsertChannelItemsAsync(IChannel channel)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.InsertChannelItemsAsync(channel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IVideoItem>> GetChannelItemsNetAsync(string parentID)
        {
            var fb = ServiceLocator.YouTubeSite;
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.GetChannelItemsAsync(parentID);
                lst.AddRange(fbres.Select(item => new VideoItem(item)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IVideoItem>> GetPopularItemsNetAsync(string regionID, int maxresult)
        {
            var fb = ServiceLocator.YouTubeSite;
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.GetPopularItemsAsync(regionID, maxresult);
                lst.AddRange(fbres.Select(item => new VideoItem(item)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IVideoItem>> SearchItemsNetAsync(string key, int maxresult)
        {
            var fb = ServiceLocator.YouTubeSite;
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.SearchItemsAsync(key, maxresult);
                lst.AddRange(fbres.Select(item => new VideoItem(item)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IPlaylist>> GetChannelPlaylistsNetAsync(string channelID)
        {
            var fb = ServiceLocator.YouTubeSite;
            try
            {
                var lst = new List<IPlaylist>();
                var fbres = await fb.GetChannelPlaylistNetAsync(channelID);
                lst.AddRange(fbres.Select(item => new Playlist(item)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IPlaylist>> GetChannelPlaylistsAsync(string channelID)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var lst = new List<IPlaylist>();
                var fbres = await fb.GetChannelPlaylistAsync(channelID);
                lst.AddRange(fbres.Select(item => new Playlist(item)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }            
        }

        public async Task<List<ITag>> GetChannelTagsAsync(string id)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var lst = new List<ITag>();
                var fbres = await fb.GetChannelTagsAsync(id);
                lst.AddRange(fbres.Select(item => new Tag(item)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertChannelTagAsync(string channelid, string tag)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.InsertChannelTagsAsync(channelid, tag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteChannelTagAsync(string channelid, string tag)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.DeleteChannelTagsAsync(channelid, tag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
    }
}
