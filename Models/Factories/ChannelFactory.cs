// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Data;
using DataAPI.POCO;
using DataAPI.Trackers;
using DataAPI.Videos;
using Extensions;
using Extensions.Helpers;
using Interfaces.Enums;
using Interfaces.Models;
using Models.BO.Channels;

namespace Models.Factories
{
    public class ChannelFactory
    {
        #region Static Methods

        public static IChannel CreateChannel(SiteType site)
        {
            IChannel channel = null;
            switch (site)
            {
                case SiteType.YouTube:
                    channel = new YouChannel { Site = site };
                    break;
            }

            if (channel == null)
            {
                throw new Exception();
            }

            channel.ChannelItemsCollectionView = CollectionViewSource.GetDefaultView(channel.ChannelItems);
            return channel;
        }

        public static IChannel CreateChannel(ChannelPOCO poco)
        {
            SiteType site = poco.Site;
            IChannel channel = null;

            switch (site)
            {
                case SiteType.YouTube:

                    channel = new YouChannel
                    {
                        ID = poco.ID,
                        Title = poco.Title,
                        SubTitle = poco.SubTitle, // .WordWrap(80);
                        Thumbnail = poco.Thumbnail,
                        Site = poco.Site,
                        CountNew = poco.Countnew
                    };

                    if (poco.Items != null)
                    {
                        foreach (VideoItemPOCO item in poco.Items)
                        {
                            channel.AddNewItem(VideoItemFactory.CreateVideoItem(item));
                        }
                    }

                    if (poco.Playlists != null)
                    {
                        foreach (PlaylistPOCO playlist in poco.Playlists)
                        {
                            channel.ChannelPlaylists.Add(PlaylistFactory.CreatePlaylist(playlist));
                        }
                    }

                    break;

                case SiteType.RuTracker:
                    channel = null;
                    break;

                case SiteType.Tapochek:
                    channel = null;
                    break;
            }

            if (channel == null)
            {
                throw new Exception(poco.ID);
            }

            channel.Site = site;
            channel.ChannelItemsCollectionView = CollectionViewSource.GetDefaultView(channel.ChannelItems);
            return channel;
        }

        public static async Task FillChannelCookieNetAsync(IChannel channel)
        {
            switch (channel.Site)
            {
                case SiteType.Tapochek:

                    TapochekSite fb = CommonFactory.CreateTapochekSite();
                    channel.ChannelCookies = await fb.GetCookieNetAsync(channel);

                    break;

                default:
                    throw new Exception(channel + " is not implemented yet");
            }
        }

        public static async Task FillChannelItemsFromDbAsync(IChannel channel, string dir, int count, int offset)
        {
            try
            {
                channel.ChannelItemsCount = await CommonFactory.CreateSqLiteDatabase().GetChannelItemsCountDbAsync(channel.ID);

                List<string> lst =
                    (await CommonFactory.CreateSqLiteDatabase().GetChannelItemsIdListDbAsync(channel.ID, count, offset)).ToList();

                if (lst.Any())
                {
                    foreach (string id in lst)
                    {
                        IVideoItem vid = await VideoItemFactory.GetVideoItemDbAsync(id);
                        vid.Site = channel.Site; // TODO rework
                        channel.ChannelItems.Add(vid);
                        vid.IsHasLocalFileFound(dir);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<IEnumerable<IVideoItem>> GetChannelItemsNetAsync(IChannel channel, int maxresult)
        {
            var lst = new List<IVideoItem>();
            switch (channel.Site)
            {
                case SiteType.YouTube:

                    IEnumerable<VideoItemPOCO> youres = await YouTubeSite.GetChannelItemsAsync(channel.ID, maxresult);
                    lst.AddRange(youres.Select(VideoItemFactory.CreateVideoItem));

                    break;

                case SiteType.Tapochek:

                    IEnumerable<VideoItemPOCO> tapres = await CommonFactory.CreateTapochekSite().GetChannelItemsAsync(channel, maxresult);
                    lst.AddRange(tapres.Select(VideoItemFactory.CreateVideoItem));

                    break;

                default:
                    throw new Exception(EnumHelper.GetAttributeOfType(channel.Site) + " is not implemented yet");
            }

            return lst;
        }

        public static async Task<IChannel> GetChannelNetAsync(string channelID, SiteType site)
        {
            ChannelPOCO poco = null;
            try
            {
                switch (site)
                {
                    case SiteType.YouTube:
                        poco = await YouTubeSite.GetChannelFullNetAsync(channelID);
                        break;
                    case SiteType.RuTracker:
                        poco = await CommonFactory.CreateRutrackerSite().GetChannelNetAsync(channelID);
                        break;
                    case SiteType.Tapochek:
                        poco = await CommonFactory.CreateTapochekSite().GetChannelNetAsync(channelID);
                        break;
                }
                IChannel channel = CreateChannel(poco);
                return channel;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<List<IPlaylist>> GetChannelPlaylistsAsync(string channelID)
        {
            var lst = new List<IPlaylist>();
            try
            {
                List<PlaylistPOCO> fbres = await CommonFactory.CreateSqLiteDatabase().GetChannelPlaylistAsync(channelID);
                lst.AddRange(fbres.Select(PlaylistFactory.CreatePlaylist));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<List<ITag>> GetChannelTagsAsync(string id)
        {
            var lst = new List<ITag>();
            try
            {
                List<TagPOCO> fbres = await CommonFactory.CreateSqLiteDatabase().GetChannelTagsAsync(id);
                lst.AddRange(fbres.Select(TagFactory.CreateTag));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<IEnumerable<IChannel>> GetRelatedChannelNetAsync(IChannel channel)
        {
            IEnumerable<ChannelPOCO> related = null;
            if (channel is YouChannel)
            {
                related = await YouTubeSite.GetRelatedChannelsByIdAsync(channel.ID);
            }

            if (related != null)
            {
                return related.Select(CreateChannel);
            }
            throw new Exception(channel.ID);
        }

        public static async Task SyncChannelAsync(IChannel channel, bool isFastSync, bool isSyncPls = false)
        {
            channel.ChannelState = ChannelState.InWork;
            if (channel is YouChannel)
            {
                string pluploadsid = YouChannel.MakePlaylistUploadId(channel.ID);

                // убираем признак предыдущей синхронизации
                List<IVideoItem> preds = channel.ChannelItems.Where(x => x.SyncState == SyncState.Added).ToList();
                if (preds.Any())
                {
                    preds.ForEach(x => x.SyncState = SyncState.Notset);
                }
                if (channel.CountNew > 0)
                {
                    channel.CountNew = 0;
                    await CommonFactory.CreateSqLiteDatabase().UpdateItemSyncState(SyncState.Notset, SyncState.Added, channel.ID);
                    await CommonFactory.CreateSqLiteDatabase().UpdateChannelNewCountAsync(channel.ID, 0);
                }

                // получаем списки id в базе и в нете
                List<string> netids;
                List<string> dbids;
                if (isFastSync)
                {
                    dbids = await CommonFactory.CreateSqLiteDatabase().GetChannelItemsIdListDbAsync(channel.ID, 0, 0, SyncState.Deleted);
                    int netcount = await YouTubeSite.GetChannelItemsCountNetAsync(channel.ID);
                    int resint = Math.Abs(netcount - dbids.Count) + 3; // буфер, можно регулировать
                    netids = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(pluploadsid, resint);
                }
                else
                {
                    dbids = await CommonFactory.CreateSqLiteDatabase().GetChannelItemsIdListDbAsync(channel.ID, 0, 0);
                    netids = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(pluploadsid, 0);

                    // проставляем в базе признак того, что видео больше нет на канале
                    foreach (string dbid in dbids.Where(dbid => !netids.Contains(dbid)))
                    {
                        await CommonFactory.CreateSqLiteDatabase().UpdateItemSyncState(dbid, SyncState.Deleted);
                    }
                }

                // cобираем новые
                List<string> trueids = netids.Where(netid => !dbids.Contains(netid)).ToList();
                IEnumerable<List<string>> tchanks = trueids.SplitList();
                foreach (List<string> list in tchanks)
                {
                    List<VideoItemPOCO> trlist = await YouTubeSite.GetVideosListByIdsLiteAsync(list);
                    IEnumerable<string> lsttru = from poco in trlist where poco.ParentID == channel.ID select poco.ID;
                    List<VideoItemPOCO> res = await YouTubeSite.GetVideosListByIdsAsync(lsttru); // получим скопом
                    foreach (IVideoItem vi in res.Select(VideoItemFactory.CreateVideoItem).Where(vi => vi.ParentID == channel.ID))
                    {
                        vi.SyncState = SyncState.Added;
                        channel.AddNewItem(vi);
                        await CommonFactory.CreateSqLiteDatabase().InsertItemAsync(vi);
                        dbids.Add(vi.ID);
                    }
                }

                // обновим инфу о количестве новых после синхронизации
                if (channel.CountNew > 0)
                {
                    await CommonFactory.CreateSqLiteDatabase().UpdateChannelNewCountAsync(channel.ID, channel.CountNew);
                }

                if (isSyncPls)
                {
                    List<string> plIdsNet = await YouTubeSite.GetChannelPlaylistsIdsNetAsync(channel.ID);
                    List<string> plIdsDb = await CommonFactory.CreateSqLiteDatabase().GetChannelsPlaylistsIdsListDbAsync(channel.ID);
                    foreach (string playlistId in plIdsDb)
                    {
                        if (plIdsNet.Contains(playlistId))
                        {
                            // обновим плейлисты, которые есть уже в базе
                            List<string> plitemsIdsNet = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(playlistId, 0);
                            List<string> plitemsIdsDb =
                                await CommonFactory.CreateSqLiteDatabase().GetPlaylistItemsIdsListDbAsync(playlistId);

                            List<string> ids = plitemsIdsNet.Where(netid => !plitemsIdsDb.Contains(netid)).ToList();
                            if (!ids.Any())
                            {
                                continue;
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
                                await CommonFactory.CreateSqLiteDatabase().UpdatePlaylistAsync(playlistId, id, channel.ID);
                            }

                            IEnumerable<List<string>> chanks = lstNoInDb.SplitList();
                            foreach (List<string> list in chanks)
                            {
                                List<VideoItemPOCO> trlist = await YouTubeSite.GetVideosListByIdsLiteAsync(list);
                                List<string> lsttru = (from poco in trlist where poco.ParentID == channel.ID select poco.ID).ToList();
                                if (!lsttru.Any())
                                {
                                    continue;
                                }

                                // странный вариант, через аплоад видео не пришло, а через плейлист - есть, но оставим
                                List<VideoItemPOCO> res = await YouTubeSite.GetVideosListByIdsAsync(lsttru); // получим скопом
                                foreach (
                                    IVideoItem vi in res.Select(VideoItemFactory.CreateVideoItem).Where(vi => vi.ParentID == channel.ID))
                                {
                                    vi.SyncState = SyncState.Added;
                                    channel.AddNewItem(vi);
                                    await CommonFactory.CreateSqLiteDatabase().InsertItemAsync(vi);
                                    await CommonFactory.CreateSqLiteDatabase().UpdatePlaylistAsync(playlistId, vi.ID, channel.ID);
                                }
                            }
                        }
                        else
                        {
                            // просто удалим уже не существующий в инете плейлист из базы
                            await CommonFactory.CreateSqLiteDatabase().DeletePlaylistAsync(playlistId);
                        }
                    }

                    // новые плейлисты
                    foreach (string playlistId in plIdsNet.Where(playlistId => !plIdsDb.Contains(playlistId)))
                    {
                        PlaylistPOCO plpoco = await YouTubeSite.GetPlaylistNetAsync(playlistId);
                        List<string> plpocoitems = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(playlistId, 0);
                        plpoco.PlaylistItems.AddRange(plpocoitems);
                        IPlaylist pl = PlaylistFactory.CreatePlaylist(plpoco);
                        channel.ChannelPlaylists.Add(pl);
                        await CommonFactory.CreateSqLiteDatabase().InsertPlaylistAsync(pl);
                        dbids = await CommonFactory.CreateSqLiteDatabase().GetChannelItemsIdListDbAsync(channel.ID, 0, 0);

                        List<string> ids = plpocoitems.Where(netid => !dbids.Contains(netid)).ToList();
                        IEnumerable<List<string>> chanks = ids.SplitList();
                        foreach (List<string> list in chanks)
                        {
                            List<VideoItemPOCO> res = await YouTubeSite.GetVideosListByIdsAsync(list); // получим скопом
                            foreach (IVideoItem vi in res.Select(VideoItemFactory.CreateVideoItem).Where(vi => vi.ParentID == channel.ID))
                            {
                                vi.SyncState = SyncState.Added;
                                channel.AddNewItem(vi);
                                await CommonFactory.CreateSqLiteDatabase().InsertItemAsync(vi);
                                await CommonFactory.CreateSqLiteDatabase().UpdatePlaylistAsync(playlistId, vi.ID, channel.ID);
                            }
                        }

                        foreach (string plpocoitem in plpocoitems)
                        {
                            await pl.UpdatePlaylistAsync(plpocoitem);
                        }
                    }
                }
            }

            channel.ChannelState = ChannelState.Notset;
        }

        public static async Task SyncChannelPlaylistsAsync(IChannel channel)
        {
            switch (channel.Site)
            {
                case SiteType.YouTube:

                    List<IPlaylist> pls = await GetChannelPlaylistsNetAsync(channel.ID);
                    if (pls.Any())
                    {
                        await CommonFactory.CreateSqLiteDatabase().DeleteChannelPlaylistsAsync(channel.ID);
                        channel.ChannelPlaylists.Clear();
                    }
                    channel.PlaylistCount = pls.Count;
                    foreach (IPlaylist playlist in pls)
                    {
                        await CommonFactory.CreateSqLiteDatabase().InsertPlaylistAsync(playlist);

                        List<string> plv = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(playlist.ID, 0);

                        foreach (string id in plv)
                        {
                            if (channel.ChannelItems.Select(x => x.ID).Contains(id))
                            {
                                await playlist.UpdatePlaylistAsync(id);
                            }
                            else
                            {
                                IVideoItem item = await VideoItemFactory.GetVideoItemNetAsync(id, channel.Site);
                                if (item.ParentID != channel.ID)
                                {
                                    continue;
                                }

                                channel.AddNewItem(item);
                                await item.InsertItemAsync();
                                await playlist.UpdatePlaylistAsync(item.ID);
                            }
                        }

                        channel.ChannelPlaylists.Add(playlist);
                    }

                    break;
            }
        }

        private static async Task<List<IPlaylist>> GetChannelPlaylistsNetAsync(string channelID)
        {
            var lst = new List<IPlaylist>();
            try
            {
                List<PlaylistPOCO> fbres = await YouTubeSite.GetChannelPlaylistsNetAsync(channelID);
                lst.AddRange(fbres.Select(PlaylistFactory.CreatePlaylist));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion
    }
}
