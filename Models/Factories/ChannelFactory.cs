// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static async Task DeleteChannelAsync(string channelID)
        {
            try
            {
                await CommonFactory.CreateSqLiteDatabase().DeleteChannelAsync(channelID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task DeleteChannelPlaylistsAsync(string channelID)
        {
            try
            {
                IEnumerable<string> lst = await CommonFactory.CreateSqLiteDatabase().GetChannelsPlaylistsIdsListDbAsync(channelID);
                foreach (string id in lst)
                {
                    await CommonFactory.CreateSqLiteDatabase().DeletePlaylistAsync(id);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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

        public static async Task<IEnumerable<IVideoItem>> GetChannelItemsNetAsync(YouChannel channel, int maxresult)
        {
            var lst = new List<IVideoItem>();
            switch (channel.Site)
            {
                case SiteType.YouTube:

                    IEnumerable<VideoItemPOCO> youres = await YouTubeSite.GetChannelItemsAsync(channel.ID, maxresult);
                    lst.AddRange(youres.Select(poco => VideoItemFactory.CreateVideoItem(poco)));

                    break;

                case SiteType.Tapochek:

                    IEnumerable<VideoItemPOCO> tapres = await CommonFactory.CreateTapochekSite().GetChannelItemsAsync(channel, maxresult);
                    lst.AddRange(tapres.Select(poco => VideoItemFactory.CreateVideoItem(poco)));

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

        public static async Task<IEnumerable<IPlaylist>> GetChannelPlaylistsAsync(string channelID)
        {
            var lst = new List<IPlaylist>();
            try
            {
                IEnumerable<PlaylistPOCO> fbres = await CommonFactory.CreateSqLiteDatabase().GetChannelPlaylistAsync(channelID);
                lst.AddRange(fbres.Select(poco => PlaylistFactory.CreatePlaylist(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<IEnumerable<IPlaylist>> GetChannelPlaylistsNetAsync(string channelID)
        {
            var lst = new List<IPlaylist>();
            try
            {
                IEnumerable<PlaylistPOCO> fbres = await YouTubeSite.GetChannelPlaylistsNetAsync(channelID);
                lst.AddRange(fbres.Select(poco => PlaylistFactory.CreatePlaylist(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<IEnumerable<ITag>> GetChannelTagsAsync(string id)
        {
            var lst = new List<ITag>();
            try
            {
                IEnumerable<TagPOCO> fbres = await CommonFactory.CreateSqLiteDatabase().GetChannelTagsAsync(id);
                lst.AddRange(fbres.Select(poco => TagFactory.CreateTag(poco)));
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

        public static async Task SyncChannelAsync(IChannel channel)
        {
            channel.ChannelState = ChannelState.InWork;
            if (channel is YouChannel)
            {
                var sb = new StringBuilder(channel.ID);
                sb[1] = 'U';
                string pluploadsid = sb.ToString();

                // теоретически, может не сработать (добавили одно, удалили одно), но в общем случае - быстрее
                // int dbcount = await sql.GetChannelItemsCountDbAsync(channel.ID);
                // int netCount = await YouTubeSite.GetPlaylistItemsCountNetAsync(pluploadsid);
                // if (dbcount == netCount)
                // {
                // return;
                // }

                // убираем признак предыдущей синхронизации
                List<IVideoItem> preds = channel.ChannelItems.Where(x => x.SyncState == SyncState.Added).ToList();
                if (preds.Any())
                {
                    preds.ForEach(x => x.SyncState = SyncState.Notset);
                }
                if (channel.CountNew > 0)
                {
                    await CommonFactory.CreateSqLiteDatabase().UpdateItemSyncState(SyncState.Notset, channel.ID);
                    await CommonFactory.CreateSqLiteDatabase().UpdateChannelNewCountAsync(channel.ID, 0);
                }

                List<string> netids = (await YouTubeSite.GetPlaylistItemsIdsListNetAsync(pluploadsid, 0)).ToList();
                List<string> dbids = (await CommonFactory.CreateSqLiteDatabase().GetChannelItemsIdListDbAsync(channel.ID, 0, 0)).ToList();
                channel.CountNew = 0;

                // проставляем в базе признак того, что видео больше нет на канале
                foreach (string dbid in dbids.Where(dbid => !netids.Contains(dbid)))
                {
                    await CommonFactory.CreateSqLiteDatabase().UpdateItemSyncState(dbid, SyncState.Deleted);
                }

                // cобираем новые
                List<string> trueids = netids.Where(netid => !dbids.Contains(netid)).ToList();
                IEnumerable<List<string>> tchanks = trueids.SplitList();
                foreach (List<string> list in tchanks)
                {
                    IEnumerable<VideoItemPOCO> res = await YouTubeSite.GetVideosListByIdsAsync(list); // получим скопом
                    foreach (
                        IVideoItem vi in res.Select(poco => VideoItemFactory.CreateVideoItem(poco)).Where(vi => vi.ParentID == channel.ID)
                        )
                    {
                        vi.SyncState = SyncState.Added;
                        channel.AddNewItem(vi);
                        await CommonFactory.CreateSqLiteDatabase().InsertItemAsync(vi);
                    }
                }

                // обновим инфу о количестве новых после синхронизации
                if (channel.CountNew > 0)
                {
                    await CommonFactory.CreateSqLiteDatabase().UpdateChannelNewCountAsync(channel.ID, channel.CountNew);
                }

                channel.ChannelItemsCount = netids.Count;
            }

            channel.ChannelState = ChannelState.Notset;
        }

        // public async Task SyncChannelAsync(YouChannel channel, bool isSyncPls)
        // {
        // channel.IsInWork = true;

        // // получаем количество записей в базе
        // // var dbCount = await GetChannelItemsCountDbAsync(YouChannel.ID);
        // List<string> idsdb = (await GetChannelItemsIdsListDbAsync(channel.ID)).ToList();

        // // получаем количество записей на канале
        // // var lsids = await YouChannel.GetChannelItemsIdsListNetAsync(0);
        // // var netCount = lsids.Count;
        // int netCount = await GetChannelItemsCountNetAsync(channel.ID);

        // if (netCount > idsdb.Count)
        // {
        // int nc = netCount - idsdb.Count + 1; // с запасом :)
        // List<string> lsidNet = (await channel.GetChannelItemsIdsListNetAsync(nc)).ToList();

        // if (lsidNet.Count != idsdb.Count && lsidNet.Any())
        // {
        // lsidNet.Reverse();
        // List<string> trueids = lsidNet.Where(id => !idsdb.Contains(id)).ToList(); // id которых нет
        // if (trueids.Any())
        // {
        // VideoItemFactory vf = commonFactory.CreateVideoItemFactory();
        // YouTubeSite you = commonFactory.CreateYouTubeSite();

        // IEnumerable<List<string>> tchanks = CommonExtensions.SplitList(trueids); // бьем на чанки - минимизируем запросы

        // foreach (List<string> list in tchanks)
        // {
        // IEnumerable<IVideoItemPOCO> res = await you.GetVideosListByIdsAsync(list); // получим скопом

        // foreach (IVideoItemPOCO poco in res)
        // {
        // IVideoItem vi = vf.CreateVideoItem(poco);
        // channel.AddNewItem(vi, true);
        // await vi.InsertItemAsync();
        // }
        // }
        // }
        // }
        // }

        // if (isSyncPls)
        // {
        // List<IPlaylist> dbpls = (await channel.GetChannelPlaylistsDbAsync()).ToList(); // получаем все плэйлисты из базы

        // List<IPlaylist> pls = (await channel.GetChannelPlaylistsNetAsync()).ToList(); // получаем все плэйлисты из сети

        // // в сети изменилось количество плэйлистов - тупо все удалим и запишем заново
        // if (dbpls.Count != pls.Count)
        // {
        // foreach (IPlaylist pl in dbpls)
        // {
        // await pl.DeletePlaylistAsync();
        // }

        // foreach (IPlaylist pl in pls)
        // {
        // await pl.InsertPlaylistAsync();

        // IEnumerable<string> plv = await pl.GetPlaylistItemsIdsListNetAsync(0);

        // foreach (string id in plv)
        // {
        // if (channel.ChannelItems.Select(x => x.ID).Contains(id))
        // {
        // await pl.UpdatePlaylistAsync(id);
        // }
        // }
        // }
        // }
        // else
        // {
        // // количество плэйлистов в базе и в сети одинаково - посмотрим на содержимое
        // foreach (IPlaylist pl in pls)
        // {
        // // получим количество видюх плейлиста в сети
        // List<string> plv = (await pl.GetPlaylistItemsIdsListNetAsync(0)).ToList();

        // // получим количество видюх плэйлиста в базе
        // IEnumerable<string> plvdb = await pl.GetPlaylistItemsIdsListDbAsync();

        // // если равно - считаем что содержимое плейлиста не изменилось (не факт конечно, но да пох)
        // if (plv.Count == plvdb.Count())
        // {
        // continue;
        // }

        // // изменилось содержимое плэйлиста - тупо удалим его (бд - каскад) и запишем с новыми данными
        // await pl.DeletePlaylistAsync();

        // await pl.InsertPlaylistAsync(); // запишем

        // foreach (string id in plv)
        // {
        // // обновим
        // if (channel.ChannelItems.Select(x => x.ID).Contains(id))
        // {
        // await pl.UpdatePlaylistAsync(id);
        // }
        // }
        // }
        // }
        // }
        // channel.IsInWork = false;
        // }
        public static async Task SyncChannelPlaylistsAsync(YouChannel channel)
        {
            List<IPlaylist> pls = (await channel.GetChannelPlaylistsNetAsync()).ToList();

            if (pls.Any())
            {
                await channel.DeleteChannelPlaylistsAsync();
                channel.ChannelPlaylists.Clear();
            }
            channel.PlaylistCount = pls.Count;
            foreach (IPlaylist playlist in pls)
            {
                await playlist.InsertPlaylistAsync();

                IEnumerable<string> plv = await playlist.GetPlaylistItemsIdsListNetAsync(0);

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
        }

        #endregion
    }
}
