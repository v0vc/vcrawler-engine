// This file contains my intellectual property. Release of this file requires prior approval from me.
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
                    channel.ChannelCookies = await fb.GetCookieNetAsync(channel);

                    break;

                default:
                    throw new Exception(channel + " is not implemented yet");
            }
        }

        public static async Task FillChannelItemsFromDbAsync(IChannel channel, int count, int offset)
        {
            channel.ChannelItemsCount = await Task.Run(() => db.GetChannelItemsCountDbAsync(channel.ID));
            List<VideoItemPOCO> items = offset == 0
                ? await Task.Run(() => db.GetChannelItemsAsync(channel.ID, count, offset))
                : await db.GetChannelItemsAsync(channel.ID, count, offset);
            foreach (IVideoItem vi in items.Select(poco => VideoItemFactory.CreateVideoItem(poco, channel.Site)))
            {
                vi.IsHasLocalFileFound(channel.DirPath);
                channel.ChannelItems.Add(vi);
            }
        }

        public static async Task<IEnumerable<IVideoItem>> GetChannelItemsNetAsync(IChannel channel, int maxresult)
        {
            var lst = new List<IVideoItem>();

            SiteType site = channel.Site;
            IEnumerable<VideoItemPOCO> res;
            switch (site)
            {
                case SiteType.YouTube:
                    res = await YouTubeSite.GetChannelItemsAsync(channel.ID, maxresult);
                    break;

                case SiteType.Tapochek:
                    res = await CommonFactory.CreateTapochekSite().GetChannelItemsAsync(channel, maxresult);
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

        //public static async Task<List<IPlaylist>> GetChannelPlaylistsAsync(string channelID)
        //{
        //    var lst = new List<IPlaylist>();
        //    try
        //    {
        //        List<PlaylistPOCO> fbres = await db.GetChannelPlaylistAsync(channelID);
        //        lst.AddRange(fbres.Select(poco => PlaylistFactory.CreatePlaylist(poco, TODO)));
        //        return lst;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        public static async Task<List<ITag>> GetChannelTagsAsync(string id)
        {
            var lst = new List<ITag>();
            try
            {
                List<TagPOCO> fbres = await db.GetChannelTagsAsync(id);
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
                return related.Select(poco => CreateChannel(poco, channel.DirPath));
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
                    await db.UpdateItemSyncState(SyncState.Notset, SyncState.Added, channel.ID);
                    await db.UpdateChannelNewCountAsync(channel.ID, 0);
                }

                // получаем списки id в базе и в нете
                List<string> netids;
                List<string> dbids;
                if (!isFastSync && !channel.UseFast)
                {
                    // полная проверка, с учетом индивдуальной опции (для больших каналов)
                    dbids = await db.GetChannelItemsIdListDbAsync(channel.ID, 0, 0);
                    netids = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(pluploadsid, 0);

                    // проставляем в базе признак того, что видео больше нет на канале
                    foreach (string dbid in dbids.Where(dbid => !netids.Contains(dbid)))
                    {
                        await db.UpdateItemSyncState(dbid, SyncState.Deleted);
                    }
                }
                else
                {
                    // быстрая проверка
                    dbids = await db.GetChannelItemsIdListDbAsync(channel.ID, 0, 0, SyncState.Deleted);
                    int netcount = await YouTubeSite.GetChannelItemsCountNetAsync(channel.ID);
                    int resint = Math.Abs(netcount - dbids.Count) + 3; // буфер, можно регулировать
                    netids = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(pluploadsid, resint);
                }

                // cобираем новые
                List<string> trueids = netids.Where(netid => !dbids.Contains(netid)).ToList();
                IEnumerable<List<string>> tchanks = trueids.SplitList();
                foreach (List<string> list in tchanks)
                {
                    List<VideoItemPOCO> trlist = await YouTubeSite.GetVideosListByIdsLiteAsync(list);
                    IEnumerable<string> trueIds = from poco in trlist where poco.ParentID == channel.ID select poco.ID;
                    await InsertNewItems(trueIds, channel, null, dbids);
                }

                // обновим инфу о количестве новых после синхронизации
                if (channel.CountNew > 0)
                {
                    await db.UpdateChannelNewCountAsync(channel.ID, channel.CountNew);
                }

                // синхронизовать также плейлисты (двойной клик с UI по каналу)
                if (isSyncPls)
                {
                    await SyncPlaylists(channel, dbids);
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

                    List<PlaylistPOCO> fbres = await YouTubeSite.GetChannelPlaylistsNetAsync(channel.ID);
                    List<IPlaylist> pls = new List<IPlaylist>();
                    pls.AddRange(fbres.Select(poco => PlaylistFactory.CreatePlaylist(poco, channel.Site)));
                    if (pls.Any())
                    {
                        await db.DeleteChannelPlaylistsAsync(channel.ID);
                        channel.ChannelPlaylists.Clear();
                    }
                    channel.PlaylistCount = pls.Count;
                    foreach (IPlaylist playlist in pls)
                    {
                        await db.InsertPlaylistAsync(playlist);

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

        //private static async Task<List<IPlaylist>> GetChannelPlaylistsNetAsync(string channelID)
        //{
        //    var lst = new List<IPlaylist>();
        //    try
        //    {
        //        List<PlaylistPOCO> fbres = await YouTubeSite.GetChannelPlaylistsNetAsync(channelID);
        //        lst.AddRange(fbres.Select(poco => PlaylistFactory.CreatePlaylist(poco, TODO)));
        //        return lst;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        private static async Task InsertNewItems(IEnumerable<string> trueIds, 
            IChannel channel, 
            string playlistId = null, 
            ICollection<string> dbIds = null)
        {
            List<VideoItemPOCO> res = await YouTubeSite.GetVideosListByIdsAsync(trueIds); // получим скопом
            IEnumerable<IVideoItem> result =
                res.Select(poco => VideoItemFactory.CreateVideoItem(poco, channel.Site))
                    .Reverse()
                    .Where(vi => vi.ParentID == channel.ID)
                    .ToList();
            result.ForEach(x => x.SyncState = SyncState.Added);
            await db.InsertChannelItemsAsync(result);
            foreach (IVideoItem vi in result)
            {
                vi.SyncState = SyncState.Added;
                channel.AddNewItem(vi);
                if (playlistId != null)
                {
                    await db.UpdatePlaylistAsync(playlistId, vi.ID, channel.ID);
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
            List<string> plIdsNet = await YouTubeSite.GetChannelPlaylistsIdsNetAsync(channel.ID);
            List<string> plIdsDb = await db.GetChannelsPlaylistsIdsListDbAsync(channel.ID);
            foreach (string playlistId in plIdsDb)
            {
                if (plIdsNet.Contains(playlistId))
                {
                    // обновим плейлисты, которые есть уже в базе
                    List<string> plitemsIdsNet = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(playlistId, 0);
                    List<string> plitemsIdsDb = await db.GetPlaylistItemsIdsListDbAsync(playlistId);

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
                        await db.UpdatePlaylistAsync(playlistId, id, channel.ID);
                    }

                    IEnumerable<List<string>> chanks = lstNoInDb.SplitList();
                    foreach (List<string> list in chanks)
                    {
                        List<VideoItemPOCO> trlist = await YouTubeSite.GetVideosListByIdsLiteAsync(list);
                        List<string> trueIds = (from poco in trlist where poco.ParentID == channel.ID select poco.ID).ToList();
                        if (!trueIds.Any())
                        {
                            continue;
                        }

                        // странный вариант, через аплоад видео не пришло, а через плейлист - есть, но оставим
                        await InsertNewItems(trueIds, channel, playlistId);
                    }
                }
                else
                {
                    // просто удалим уже не существующий в инете плейлист из базы
                    await db.DeletePlaylistAsync(playlistId);
                }
            }

            // новые плейлисты
            foreach (string playlistId in plIdsNet.Where(playlistId => !plIdsDb.Contains(playlistId)))
            {
                PlaylistPOCO plpoco = await YouTubeSite.GetPlaylistNetAsync(playlistId);
                List<string> plpocoitems = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(playlistId, 0);
                plpoco.PlaylistItems.AddRange(plpocoitems);
                IPlaylist pl = PlaylistFactory.CreatePlaylist(plpoco, channel.Site);
                pl.State = SyncState.Added;
                channel.ChannelPlaylists.Add(pl);
                channel.PlaylistCount += 1;
                await db.InsertPlaylistAsync(pl);
                dbids = await db.GetChannelItemsIdListDbAsync(channel.ID, 0, 0);

                List<string> ids = plpocoitems.Where(netid => !dbids.Contains(netid)).ToList();
                IEnumerable<List<string>> chanks = ids.SplitList();
                foreach (List<string> trueIds in chanks)
                {
                    await InsertNewItems(trueIds, channel, playlistId);
                }

                foreach (string plpocoitem in plpocoitems)
                {
                    await pl.UpdatePlaylistAsync(plpocoitem);
                }
            }
        }

        #endregion
    }
}
