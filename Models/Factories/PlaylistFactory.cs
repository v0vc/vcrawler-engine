// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.Database;
using DataAPI.POCO;
using DataAPI.Videos;
using Extensions;
using Interfaces.Enums;
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
                Site = poco.Site,
                PlItems = poco.PlaylistItems
            };
            return pl;
        }

        public static async Task DownloadPlaylist(IPlaylist playlist)
        {
            switch (playlist.Site)
            {
                case SiteType.YouTube:

                    throw new Exception();

                    break;
            }
        }

        public static async Task UpdatePlaylist(IPlaylist playlist, IChannel selectedChannel)
        {
            //selectedChannel.ChannelItemsCollectionView.Filter = null;

            switch (playlist.Site)
            {
                case SiteType.YouTube:

                    var dbids = selectedChannel.ChannelItems.Select(x => x.ID).ToHashSet();
                    List<string> plitemsIdsNet = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(playlist.ID, 0);
                    List<string> ids = plitemsIdsNet.Where(netid => !playlist.PlItems.Contains(netid)).ToList();
                    if (!ids.Any())
                    {
                        return;
                    }
                    var lstInDb = new List<string>();
                    var lstNoInDb = new List<string>();
                    foreach (string id in ids)
                    {
                        if (dbids.Contains(id))
                        {
                            lstInDb.Add(id);
                        }
                        else
                        {
                            lstNoInDb.Add(id);
                        }
                    }
                    foreach (string id in lstInDb)
                    {
                        await CommonFactory.CreateSqLiteDatabase().UpdatePlaylistAsync(playlist.ID, id, selectedChannel.ID);
                        playlist.PlItems.Add(id);
                    }

                    IEnumerable<List<string>> chanks = lstNoInDb.SplitList();
                    foreach (List<string> list in chanks)
                    {
                        List<VideoItemPOCO> res = await YouTubeSite.GetVideosListByIdsAsync(list); // получим скопом
                        foreach (IVideoItem vi in res.Select(VideoItemFactory.CreateVideoItem))
                        {
                            vi.SyncState = SyncState.Added;
                            if (vi.ParentID == selectedChannel.ID)
                            {
                                selectedChannel.AddNewItem(vi);
                                await CommonFactory.CreateSqLiteDatabase().InsertItemAsync(vi);
                                await CommonFactory.CreateSqLiteDatabase().UpdatePlaylistAsync(playlist.ID, vi.ID, selectedChannel.ID);
                            }
                            else
                            {
                                selectedChannel.ChannelItems.Add(vi);
                            }
                            playlist.PlItems.Add(vi.ID);
                        }
                    }

                    break;
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
