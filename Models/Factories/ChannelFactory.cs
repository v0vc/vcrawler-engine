// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using DataAPI.Database;
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
    public static class ChannelFactory
    {
        #region Static and Readonly Fields

        private static readonly SqLiteDatabase db = CommonFactory.CreateSqLiteDatabase();

        #endregion

        #region Static Methods

        public static IChannel CreateChannel(SiteType site)
        {
            IChannel channel = null;
            switch (site)
            {
                case SiteType.YouTube:
                    channel = new YouChannel();
                    break;
            }

            if (channel == null)
            {
                throw new Exception();
            }

            channel.ChannelItemsCollectionView = CollectionViewSource.GetDefaultView(channel.ChannelItems);
            return channel;
        }

        public static IChannel CreateChannel(ChannelPOCO poco, string dirPath = null)
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
                        CountNew = poco.Countnew,
                        UseFast = poco.UseFast
                    };

                    if (poco.Items != null)
                    {
                        foreach (VideoItemPOCO item in poco.Items)
                        {
                            channel.AddNewItem(VideoItemFactory.CreateVideoItem(item, site));
                        }
                    }

                    if (poco.Playlists != null)
                    {
                        foreach (PlaylistPOCO playlist in poco.Playlists)
                        {
                            channel.ChannelPlaylists.Add(PlaylistFactory.CreatePlaylist(playlist, site));
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

            if (dirPath != null)
            {
                channel.DirPath = dirPath;
            }
            channel.ChannelItemsCollectionView = CollectionViewSource.GetDefaultView(channel.ChannelItems);
            return channel;
        }

        public static async Task FillChannelCookieNetAsync(IChannel channel)
        {
            switch (channel.Site)
            {
                case SiteType.Tapochek:

                    TapochekSite fb = CommonFactory.CreateTapochekSite();
                    channel.ChannelCookies = await fb.GetCookieNetAsync(channel).ConfigureAwait(false);

                    break;

                default:
                    throw new Exception(channel + " is not implemented yet");
            }
        }

        public static async void FillChannelItemsFromDbAsync(IChannel channel, int basePage, List<string> excepted = null)
        {
            List<VideoItemPOCO> items =
                await Task.Run(() => db.GetChannelItemsBaseAsync(channel.ID, basePage, excepted)).ConfigureAwait(true);

            foreach (VideoItemPOCO poco in items)
            {
                IVideoItem vi = VideoItemFactory.CreateVideoItem(poco, channel.Site);
                vi.IsHasLocalFileFound(channel.DirPath);
                channel.ChannelItems.Add(vi);
            }
            channel.RefreshView("Timestamp");
        }

        public static async Task<IEnumerable<IVideoItem>> GetChannelItemsNetAsync(IChannel channel, int maxresult)
        {
            var lst = new List<IVideoItem>();

            SiteType site = channel.Site;
            IEnumerable<VideoItemPOCO> res;
            switch (site)
            {
                case SiteType.YouTube:
                    res = await YouTubeSite.GetChannelItemsAsync(channel.ID, maxresult).ConfigureAwait(false);
                    break;

                case SiteType.Tapochek:
                    res = await CommonFactory.CreateTapochekSite().GetChannelItemsAsync(channel, maxresult).ConfigureAwait(false);
                    break;

                default:
                    throw new Exception(EnumHelper.GetAttributeOfType(channel.Site) + " is not implemented yet");
            }

            lst.AddRange(res.Select(poco => VideoItemFactory.CreateVideoItem(poco, site)));
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
                        poco = await YouTubeSite.GetChannelFullNetAsync(channelID).ConfigureAwait(false);
                        break;
                    case SiteType.RuTracker:
                        poco = await CommonFactory.CreateRutrackerSite().GetChannelNetAsync(channelID).ConfigureAwait(false);
                        break;
                    case SiteType.Tapochek:
                        poco = await CommonFactory.CreateTapochekSite().GetChannelNetAsync(channelID).ConfigureAwait(false);
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

        public static async Task<List<ITag>> GetChannelTagsAsync(string id)
        {
            var lst = new List<ITag>();
            try
            {
                List<TagPOCO> fbres = await db.GetChannelTagsAsync(id).ConfigureAwait(false);
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
                related = await YouTubeSite.GetRelatedChannelsByIdAsync(channel.ID).ConfigureAwait(false);
            }

            if (related != null)
            {
                return related.Select(poco => CreateChannel(poco, channel.DirPath));
            }
            throw new Exception(channel.ID);
        }

        public static async void SetChannelCountAsync(IChannel channel)
        {
            channel.ChannelItemsCount = await db.GetChannelItemsCountDbAsync(channel.ID).ConfigureAwait(false);
            if (channel.PlaylistCount == 0)
            {
                channel.PlaylistCount = await db.GetChannelPlaylistCountDbAsync(channel.ID).ConfigureAwait(false);
            }
        }

        public static async Task SyncChannelAsync(IChannel channel,
            bool isFastSync,
            bool isSyncPls = false,
            Action<IVideoItem, object> stateAction = null)
        {
            channel.ChannelState = ChannelState.InWork;
            if (channel is YouChannel)
            {
                string pluploadsid = YouChannel.MakePlaylistUploadId(channel.ID);

                // убираем признак предыдущей синхронизации
                List<IVideoItem> preds = channel.ChannelItems.Where(x => x.SyncState == SyncState.Added).ToList();
                foreach (IVideoItem item in preds)
                {
                    item.SyncState = SyncState.Notset;
                    if (stateAction != null)
                    {
                        stateAction.Invoke(item, SyncState.Notset);
                    }
                }
                if (channel.CountNew > 0)
                {
                    channel.CountNew = 0;
                    await db.UpdateItemSyncState(SyncState.Notset, SyncState.Added, channel.ID).ConfigureAwait(false);
                    await db.UpdateChannelNewCountAsync(channel.ID, 0).ConfigureAwait(false);
                }

                // получаем списки id в базе и в нете
                List<string> netids;
                List<string> dbids;
                if (!isFastSync && !channel.UseFast)
                {
                    // полная проверка, с учетом индивдуальной опции (для больших каналов)
                    dbids = await db.GetChannelItemsIdListDbAsync(channel.ID, 0, 0).ConfigureAwait(false);
                    netids = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(pluploadsid, 0).ConfigureAwait(true);

                    // проставляем в базе признак того, что видео больше нет на канале, а так же если видео было удалено, а теперь вернулось
                    List<string> deletedlist =
                        await db.GetChannelItemsIdsByStateAsync(SyncState.Deleted, channel.ID).ConfigureAwait(false);
                    foreach (string dbid in dbids)
                    {
                        if (!netids.Contains(dbid))
                        {
                            await db.UpdateItemSyncState(dbid, SyncState.Deleted).ConfigureAwait(false);
                        }
                        else
                        {
                            if (deletedlist.Any() && deletedlist.Contains(dbid))
                            {
                                await db.UpdateItemSyncState(dbid, SyncState.Notset).ConfigureAwait(false);
                            }
                        }
                    }
                }
                else
                {
                    // быстрая проверка
                    dbids = await db.GetChannelItemsIdListDbAsync(channel.ID, 0, 0, SyncState.Deleted).ConfigureAwait(false);
                    int netcount = await YouTubeSite.GetChannelItemsCountNetAsync(channel.ID).ConfigureAwait(true);
                    int resint = Math.Abs(netcount - dbids.Count) + 3; // буфер, можно регулировать
                    netids = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(pluploadsid, resint).ConfigureAwait(true);
                }

                // cобираем новые
                List<string> trueids = netids.Where(netid => !dbids.Contains(netid)).ToList();
                IEnumerable<List<string>> tchanks = trueids.SplitList();
                foreach (List<string> list in tchanks)
                {
                    List<VideoItemPOCO> trlist = await YouTubeSite.GetVideosListByIdsLiteAsync(list).ConfigureAwait(true);
                    IEnumerable<string> trueIds = from poco in trlist where poco.ParentID == channel.ID select poco.ID;
                    await InsertNewItems(trueIds, channel, null, dbids, stateAction).ConfigureAwait(true);
                }

                // обновим инфу о количестве новых после синхронизации
                if (channel.CountNew > 0)
                {
                    await db.UpdateChannelNewCountAsync(channel.ID, channel.CountNew).ConfigureAwait(false);
                }

                // синхронизовать также плейлисты (двойной клик с UI по каналу)
                if (isSyncPls)
                {
                    await SyncPlaylists(channel, dbids).ConfigureAwait(true);
                }
            }
            channel.IsHasNewFromSync = channel.ChannelItems.Any()
                                       && channel.ChannelItems.Count == channel.ChannelItems.Count(x => x.SyncState == SyncState.Added);
            channel.ChannelState = ChannelState.Notset;
        }

        public static async Task SyncChannelPlaylistsAsync(IChannel channel)
        {
            switch (channel.Site)
            {
                case SiteType.YouTube:

                    List<PlaylistPOCO> fbres = await YouTubeSite.GetChannelPlaylistsNetAsync(channel.ID).ConfigureAwait(false);
                    var pls = new List<IPlaylist>();
                    pls.AddRange(fbres.Select(poco => PlaylistFactory.CreatePlaylist(poco, channel.Site)));
                    if (pls.Any())
                    {
                        await db.DeleteChannelPlaylistsAsync(channel.ID).ConfigureAwait(false);
                        channel.ChannelPlaylists.Clear();
                    }
                    channel.PlaylistCount = pls.Count;
                    foreach (IPlaylist playlist in pls)
                    {
                        await db.InsertPlaylistAsync(playlist).ConfigureAwait(false);

                        List<string> plv = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(playlist.ID, 0).ConfigureAwait(false);

                        foreach (string id in plv)
                        {
                            if (channel.ChannelItems.Select(x => x.ID).Contains(id))
                            {
                                await db.UpdatePlaylistAsync(playlist.ID, id, channel.ID).ConfigureAwait(false);
                            }
                            else
                            {
                                IVideoItem item = await VideoItemFactory.GetVideoItemNetAsync(id, channel.Site).ConfigureAwait(false);
                                if (item.ParentID != channel.ID)
                                {
                                    continue;
                                }

                                channel.AddNewItem(item);
                                await db.InsertItemAsync(item).ConfigureAwait(false);
                                await db.UpdatePlaylistAsync(playlist.ID, item.ID, channel.ID).ConfigureAwait(false);
                            }
                        }

                        channel.ChannelPlaylists.Add(playlist);
                    }

                    break;
            }
        }

        private static async Task InsertNewItems(IEnumerable<string> trueIds,
            IChannel channel,
            string playlistId = null,
            ICollection<string> dbIds = null,
            Action<IVideoItem, object> stateAction = null)
        {
            List<VideoItemPOCO> res = await YouTubeSite.GetVideosListByIdsAsync(trueIds).ConfigureAwait(true); // получим скопом
            IEnumerable<IVideoItem> result =
                res.Select(poco => VideoItemFactory.CreateVideoItem(poco, channel.Site, SyncState.Added))
                    .Reverse()
                    .Where(vi => vi.ParentID == channel.ID)
                    .ToList();
            await db.InsertChannelItemsAsync(result).ConfigureAwait(false);
            foreach (IVideoItem vi in result)
            {
                channel.AddNewItem(vi);
                if (stateAction != null)
                {
                    stateAction.Invoke(vi, SyncState.Added);
                }
                if (playlistId != null)
                {
                    await db.UpdatePlaylistAsync(playlistId, vi.ID, channel.ID).ConfigureAwait(false);
                }
                if (dbIds == null)
                {
                    continue;
                }
                if (!dbIds.Contains(vi.ID))
                {
                    dbIds.Add(vi.ID);
                }
            }
        }

        private static async Task SyncPlaylists(IChannel channel, List<string> dbids)
        {
            List<string> plIdsNet = await YouTubeSite.GetChannelPlaylistsIdsNetAsync(channel.ID).ConfigureAwait(true);
            List<string> plIdsDb = await db.GetChannelsPlaylistsIdsListDbAsync(channel.ID).ConfigureAwait(false);
            foreach (string playlistId in plIdsDb)
            {
                if (plIdsNet.Contains(playlistId))
                {
                    // обновим плейлисты, которые есть уже в базе
                    List<string> plitemsIdsNet = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(playlistId, 0).ConfigureAwait(true);
                    List<string> plitemsIdsDb = await db.GetPlaylistItemsIdsListDbAsync(playlistId).ConfigureAwait(false);

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
                        await db.UpdatePlaylistAsync(playlistId, id, channel.ID).ConfigureAwait(false);
                    }

                    IEnumerable<List<string>> chanks = lstNoInDb.SplitList();
                    foreach (List<string> list in chanks)
                    {
                        List<VideoItemPOCO> trlist = await YouTubeSite.GetVideosListByIdsLiteAsync(list).ConfigureAwait(true);
                        List<string> trueIds = (from poco in trlist where poco.ParentID == channel.ID select poco.ID).ToList();
                        if (!trueIds.Any())
                        {
                            continue;
                        }

                        // странный вариант, через аплоад видео не пришло, а через плейлист - есть, но оставим
                        await InsertNewItems(trueIds, channel, playlistId).ConfigureAwait(true);
                    }
                }
                else
                {
                    // просто удалим уже не существующий в инете плейлист из базы
                    await db.DeletePlaylistAsync(playlistId).ConfigureAwait(false);
                }
            }

            // новые плейлисты
            foreach (string playlistId in plIdsNet.Where(playlistId => !plIdsDb.Contains(playlistId)))
            {
                PlaylistPOCO plpoco = await YouTubeSite.GetPlaylistNetAsync(playlistId).ConfigureAwait(true);
                List<string> plpocoitems = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(playlistId, 0).ConfigureAwait(true);
                plpoco.PlaylistItems.AddRange(plpocoitems);
                IPlaylist pl = PlaylistFactory.CreatePlaylist(plpoco, channel.Site);
                pl.State = SyncState.Added;
                channel.ChannelPlaylists.Add(pl);
                channel.PlaylistCount += 1;
                await db.InsertPlaylistAsync(pl).ConfigureAwait(false);
                dbids = await db.GetChannelItemsIdListDbAsync(channel.ID, 0, 0).ConfigureAwait(false);

                List<string> ids = plpocoitems.Where(netid => !dbids.Contains(netid)).ToList();
                IEnumerable<List<string>> chanks = ids.SplitList();
                foreach (List<string> trueIds in chanks)
                {
                    await InsertNewItems(trueIds, channel, playlistId).ConfigureAwait(true);
                }

                foreach (string plpocoitem in plpocoitems)
                {
                    await db.UpdatePlaylistAsync(pl.ID, plpocoitem, channel.ID).ConfigureAwait(false);
                }
            }
        }

        #endregion
    }
}
