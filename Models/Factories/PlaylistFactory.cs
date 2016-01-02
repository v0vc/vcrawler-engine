// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.Database;
using DataAPI.POCO;
using DataAPI.Videos;
using Interfaces.Models;
using Models.BO;

namespace Models.Factories
{
    public class PlaylistFactory
    {
        #region Static Methods

        public static IPlaylist CreatePlaylist()
        {
            var pl = new Playlist();
            return pl;
        }

        public static IPlaylist CreatePlaylist(PlaylistPOCO poco)
        {
            if (poco == null)
            {
                return null;
            }
            var pl = new Playlist
            {
                ID = poco.ID, 
                Title = poco.Title, 
                SubTitle = poco.SubTitle, 
                Thumbnail = poco.Thumbnail, 
                ChannelId = poco.ChannelID,
                PlItems = poco.PlaylistItems
            };
            return pl;
        }

        public static async Task DeletePlaylistAsync(string id)
        {
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
            try
            {
                await fb.DeletePlaylistAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<List<IVideoItem>> GetPlaylistItemsDbAsync(string id, string channelID)
        {
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.GetPlaylistItemsAsync(id, channelID);
                lst.AddRange(fbres.Select(VideoItemFactory.CreateVideoItem));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<IEnumerable<string>> GetPlaylistItemsIdsListDbAsync(string id)
        {
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
            try
            {
                return await fb.GetPlaylistItemsIdsListDbAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<IEnumerable<string>> GetPlaylistItemsIdsListNetAsync(string id, int maxResult)
        {
            try
            {
                return await YouTubeSite.GetPlaylistItemsIdsListNetAsync(id, maxResult);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<IEnumerable<IVideoItem>> GetPlaylistItemsNetAsync(Playlist playlist)
        {
            VideoItemFactory vf = CommonFactory.CreateVideoItemFactory();
            try
            {
                var lst = new List<IVideoItem>();
                IEnumerable<VideoItemPOCO> fbres = await YouTubeSite.GetPlaylistItemsNetAsync(playlist.ID);
                lst.AddRange(fbres.Select(VideoItemFactory.CreateVideoItem));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task InsertPlaylistAsync(Playlist playlist)
        {
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
            try
            {
                await fb.InsertPlaylistAsync(playlist);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task UpdatePlaylistAsync(string plid, string itemid, string channelid)
        {
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
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
    }
}
