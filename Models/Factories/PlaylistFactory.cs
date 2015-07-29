using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO;

namespace Models.Factories
{
    public class PlaylistFactory : IPlaylistFactory
    {
        private readonly ICommonFactory _c;

        public PlaylistFactory(ICommonFactory c)
        {
            _c = c;
        }

        public IPlaylist CreatePlaylist()
        {
            var pl = new Playlist(this) { PlaylistItems = new List<IVideoItem>() };
            return pl;
        }

        public IPlaylist CreatePlaylist(IPlaylistPOCO poco)
        {
            var pl = new Playlist(this)
            {
                ID = poco.ID,
                Title = poco.Title,
                SubTitle = poco.SubTitle,
                Thumbnail = poco.Thumbnail,
                ChannelId = poco.ChannelID,
                PlaylistItems = new List<IVideoItem>()
            };
            return pl;
        }

        public async Task<IPlaylist> GetPlaylistDbAsync(string id)
        {
            // var fb = ServiceLocator.SqLiteDatabase;
            var fb = _c.CreateSqLiteDatabase();
            var pf = _c.CreatePlaylistFactory();

            try
            {
                var poco = await fb.GetPlaylistAsync(id);
                var pl = pf.CreatePlaylist(poco);
                return pl;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IPlaylist> GetPlaylistNetAsync(string id)
        {
            var fb = _c.CreateYouTubeSite();
            var pf = _c.CreatePlaylistFactory();
            try
            {
                var fbres = await fb.GetPlaylistNetAsync(id);
                var pl = pf.CreatePlaylist(fbres);
                return pl;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IVideoItem>> GetPlaylistItemsNetAsync(Playlist playlist)
        {
            var fb = _c.CreateYouTubeSite();
            var vf = _c.CreateVideoItemFactory();
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.GetPlaylistItemsNetAsync(playlist.ID);
                lst.AddRange(fbres.Select(poco => vf.CreateVideoItem(poco)));
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
            var vf = _c.CreateVideoItemFactory();
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.GetPlaylistItemsAsync(id, channelID);
                lst.AddRange(fbres.Select(poco => vf.CreateVideoItem(poco)));
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
