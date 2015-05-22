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
    public class PlaylistFactory : IPlaylistFactory
    {
        private readonly ICommonFactory _c;

        //private readonly ISqLiteDatabase _db;

        //private readonly IYouTubeSite _youTubeSite;

        public PlaylistFactory(ICommonFactory c)
        {
            _c = c;
            //_db = c.CreateSqLiteDatabase();
            //_youTubeSite = c.CreateYouTubeSite();
        }

        public IPlaylist CreatePlaylist()
        {
            return new Playlist(this);
        }

        public async Task<IPlaylist> GetPlaylistDbAsync(string id)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var fbres = await fb.GetPlaylistAsync(id);
                return new Playlist(fbres, _c.CreatePlaylistFactory());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IPlaylist> GetPlaylistNetAsync(string id)
        {
            var fb = _c.CreateYouTubeSite();
            //var fb = ServiceLocator.YouTubeSiteApiV2;
            try
            {
                var fbres = await fb.GetPlaylistNetAsync(id);
                return new Playlist(fbres, _c.CreatePlaylistFactory());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IVideoItem>> GetPlaylistItemsNetAsync(Playlist playlist)
        {
            var fb = _c.CreateYouTubeSite();
            //var fb = ServiceLocator.YouTubeSiteApiV2;
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.GetPlaylistItemsNetAsync(playlist.ID);
                lst.AddRange(fbres.Select(item => new VideoItem(item, _c.CreateVideoItemFactory())));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IVideoItem>> GetPlaylistItemsDbAsync(string id, string channelID)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.GetPlaylistItemsAsync(id, channelID);
                lst.AddRange(fbres.Select(item => new VideoItem(item, _c.CreateVideoItemFactory())));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeletePlaylistAsync(string id)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.DeletePlaylistAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertPlaylistAsync(Playlist playlist)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.InsertPlaylistAsync(playlist);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<string>> GetPlaylistItemsIdsListNetAsync(string id)
        {
            var fb = _c.CreateYouTubeSite();
            //var fb = ServiceLocator.YouTubeSiteApiV2;
            try
            {
                return await fb.GetPlaylistItemsIdsListNetAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdatePlaylistAsync(string plid, string itemid, string channelid)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.UpdatePlaylistAsync(plid, itemid, channelid);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<string>> GetPlaylistItemsIdsListDbAsync(string id)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.YouTubeSiteApiV2;
            try
            {
                return await fb.GetPlaylistItemsIdsListDbAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
