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
    class PlaylistFactory : IPlaylistFactory
    {
        public IPlaylist CreatePlaylist()
        {
            return new Playlist();
        }

        public async Task<IPlaylist> GetPlaylistDbAsync(string id)
        {

            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var fbres = await fb.GetPlaylistAsync(id);
                return new Playlist(fbres);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IPlaylist> GetPlaylistNetAsync(string id)
        {
            var fb = ServiceLocator.YouTubeSite;
            try
            {
                var fbres = await fb.GetPlaylistNetAsync(id);
                return new Playlist(fbres);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IVideoItem>> GetPlaylistItemsNetAsync(Playlist playlist)
        {
            var fb = ServiceLocator.YouTubeSite;
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.GetPlaylistItemsNetAsync(playlist.Link);
                lst.AddRange(fbres.Select(item => new VideoItem(item)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IVideoItem>> GetPlaylistItemsDbAsync(string id, string channelID)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.GetPlaylistItemsAsync(id, channelID);
                lst.AddRange(fbres.Select(item => new VideoItem(item)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeletePlaylistAsync(string id)
        {
            var fb = ServiceLocator.SqLiteDatabase;
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
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.InsertPlaylistAsync(playlist);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
