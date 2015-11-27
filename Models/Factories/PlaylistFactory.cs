// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interfaces.API;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO;

namespace Models.Factories
{
    public class PlaylistFactory : IPlaylistFactory
    {
        #region Static and Readonly Fields

        private readonly ICommonFactory _c;

        #endregion

        #region Constructors

        public PlaylistFactory(ICommonFactory c)
        {
            _c = c;
        }

        #endregion

        #region Methods

        public async Task DeletePlaylistAsync(string id)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.DeletePlaylistAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IVideoItem>> GetPlaylistItemsDbAsync(string id, string channelID)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            IVideoItemFactory vf = _c.CreateVideoItemFactory();
            try
            {
                var lst = new List<IVideoItem>();
                IEnumerable<IVideoItemPOCO> fbres = await fb.GetPlaylistItemsAsync(id, channelID);
                lst.AddRange(fbres.Select(poco => vf.CreateVideoItem(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<string>> GetPlaylistItemsIdsListDbAsync(string id)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                return await fb.GetPlaylistItemsIdsListDbAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<string>> GetPlaylistItemsIdsListNetAsync(string id)
        {
            IYouTubeSite fb = _c.CreateYouTubeSite();
            try
            {
                return await fb.GetPlaylistItemsIdsListNetAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IVideoItem>> GetPlaylistItemsNetAsync(Playlist playlist)
        {
            IYouTubeSite fb = _c.CreateYouTubeSite();
            IVideoItemFactory vf = _c.CreateVideoItemFactory();
            try
            {
                var lst = new List<IVideoItem>();
                IEnumerable<IVideoItemPOCO> fbres = await fb.GetPlaylistItemsNetAsync(playlist.ID);
                lst.AddRange(fbres.Select(poco => vf.CreateVideoItem(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertPlaylistAsync(Playlist playlist)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.InsertPlaylistAsync(playlist);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdatePlaylistAsync(string plid, string itemid, string channelid)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.UpdatePlaylistAsync(plid, itemid, channelid);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region IPlaylistFactory Members

        public IPlaylist CreatePlaylist()
        {
            var pl = new Playlist(this);
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
            };
            return pl;
        }

        public async Task<IPlaylist> GetPlaylistDbAsync(string id)
        {
            // var fb = ServiceLocator.SqLiteDatabase;
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            IPlaylistFactory pf = _c.CreatePlaylistFactory();

            try
            {
                IPlaylistPOCO poco = await fb.GetPlaylistAsync(id);
                IPlaylist pl = pf.CreatePlaylist(poco);
                return pl;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IPlaylist> GetPlaylistNetAsync(string id)
        {
            IYouTubeSite fb = _c.CreateYouTubeSite();
            IPlaylistFactory pf = _c.CreatePlaylistFactory();
            try
            {
                IPlaylistPOCO fbres = await fb.GetPlaylistNetAsync(id);
                IPlaylist pl = pf.CreatePlaylist(fbres);
                return pl;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion
    }
}
