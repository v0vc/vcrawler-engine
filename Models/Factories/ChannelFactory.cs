﻿// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Data;
using DataAPI.Database;
using DataAPI.Trackers;
using DataAPI.Videos;
using Extensions;
using Interfaces.Enums;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO.Channels;

namespace Models.Factories
{
    public class ChannelFactory
    {
        #region Static and Readonly Fields

        private readonly CommonFactory commonFactory;
        private readonly PlaylistFactory pf;
        private readonly SqLiteDatabase sql;
        private readonly VideoItemFactory vf;
        private readonly YouTubeSite you;

        #endregion

        #region Constructors

        public ChannelFactory(CommonFactory commonFactory)
        {
            this.commonFactory = commonFactory;
            you = commonFactory.CreateYouTubeSite();
            vf = commonFactory.CreateVideoItemFactory();
            sql = commonFactory.CreateSqLiteDatabase();
            pf = commonFactory.CreatePlaylistFactory();
        }

        #endregion

        #region Methods

        public IChannel CreateChannel()
        {
            var channel = new YouChannel(this)
            {
                ChannelItems = new ObservableCollection<IVideoItem>(), 
                ChannelPlaylists = new ObservableCollection<IPlaylist>(), 
                ChannelTags = new ObservableCollection<ITag>(), 
                ChannelCookies = new CookieContainer(), 
            };
            channel.ChannelItemsCollectionView = CollectionViewSource.GetDefaultView(channel.ChannelItems);
            return channel;
        }

        public IChannel CreateChannel(SiteType site)
        {
            var channel = new YouChannel(this)
            {
                ChannelItems = new ObservableCollection<IVideoItem>(), 
                ChannelPlaylists = new ObservableCollection<IPlaylist>(), 
                ChannelTags = new ObservableCollection<ITag>(), 
                ChannelCookies = new CookieContainer(), 
                Site = site, 
                SiteAdress = CommonExtensions.GetSiteAdress(site)
            };
            channel.ChannelItemsCollectionView = CollectionViewSource.GetDefaultView(channel.ChannelItems);
            return channel;
        }

        public IChannel CreateChannel(IChannelPOCO poco)
        {
            SiteType site = CommonExtensions.GetSiteType(poco.Site);
            IChannel channel = null;

            switch (site)
            {
                case SiteType.YouTube:

                    channel = new YouChannel(this)
                    {
                        ID = poco.ID, 
                        Title = poco.Title, 
                        SubTitle = poco.SubTitle, // .WordWrap(80);
                        Thumbnail = poco.Thumbnail, 
                        SiteAdress = poco.Site, 
                        ChannelItems = new ObservableCollection<IVideoItem>(), 
                        ChannelPlaylists = new ObservableCollection<IPlaylist>(), 
                        ChannelTags = new ObservableCollection<ITag>(), 
                        ChannelCookies = new CookieContainer()
                    };

                    if (poco.Items != null)
                    {
                        foreach (IVideoItemPOCO item in poco.Items)
                        {
                            channel.AddNewItem(vf.CreateVideoItem(item), false);
                        }
                    }

                    if (poco.Playlists != null)
                    {
                        foreach (IPlaylistPOCO playlist in poco.Playlists)
                        {
                            channel.ChannelPlaylists.Add(pf.CreatePlaylist(playlist));
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

            channel.ChannelItemsCollectionView = CollectionViewSource.GetDefaultView(channel.ChannelItems);
            return channel;
        }

        public async Task DeleteChannelAsync(string channelID)
        {
            try
            {
                await sql.DeleteChannelAsync(channelID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteChannelPlaylistsAsync(string channelID)
        {
            try
            {
                IEnumerable<string> lst = await sql.GetChannelsPlaylistsIdsListDbAsync(channelID);
                foreach (string id in lst)
                {
                    await sql.DeletePlaylistAsync(id);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteChannelTagAsync(string channelid, string tag)
        {
            try
            {
                await sql.DeleteChannelTagsAsync(channelid, tag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void FillChannelCookieDb(IChannel channel)
        {
            try
            {
                channel.ChannelCookies = sql.ReadCookies(channel.SiteAdress);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task FillChannelCookieNetAsync(IChannel channel)
        {
            switch (channel.Site)
            {
                case SiteType.Tapochek:

                    TapochekSite fb = commonFactory.CreateTapochekSite();
                    channel.ChannelCookies = await fb.GetCookieNetAsync(channel);

                    break;

                default:
                    throw new Exception(channel + " is not implemented yet");
            }
        }

        public async Task FillChannelDescriptionAsync(IChannel channel)
        {
            channel.SubTitle = await sql.GetChannelDescriptionAsync(channel.ID);
        }

        public async Task FillChannelItemsFromDbAsync(IChannel channel, string dir, int count, int offset)
        {
            try
            {
                channel.ChannelItemsCount = await sql.GetChannelItemsCountDbAsync(channel.ID);

                List<string> lst = (await sql.GetChannelItemsIdListDbAsync(channel.ID, count, offset)).ToList();

                if (lst.Any())
                {
                    foreach (string id in lst)
                    {
                        IVideoItem vid = await vf.GetVideoItemDbAsync(id);
                        channel.AddNewItem(vid, false);
                        vid.IsHasLocalFileFound(dir);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ICred> GetChannelCredentialsAsync(IChannel channel)
        {
            CredFactory cf = commonFactory.CreateCredFactory();
            try
            {
                return await cf.GetCredDbAsync(channel.Site);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IChannel> GetChannelDbAsync(string channelID)
        {
            // var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                IChannelPOCO poco = await sql.GetChannelAsync(channelID);
                IChannel channel = CreateChannel(poco);
                return channel;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IVideoItem>> GetChannelItemsDbAsync(string channelID, int count, int offset)
        {
            var lst = new List<IVideoItem>();
            try
            {
                IEnumerable<IVideoItemPOCO> fbres = await sql.GetChannelItemsAsync(channelID, count, offset);
                lst.AddRange(fbres.Select(poco => vf.CreateVideoItem(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<string>> GetChannelItemsIdsListDbAsync(string channelID)
        {
            try
            {
                return await sql.GetChannelItemsIdListDbAsync(channelID, 0, 0);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<string>> GetChannelItemsIdsListNetAsync(string channelID, int maxResult)
        {
            try
            {
                return await you.GetChannelItemsIdsListNetAsync(channelID, maxResult);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IVideoItem>> GetChannelItemsNetAsync(YouChannel channel, int maxresult)
        {
            var lst = new List<IVideoItem>();
            switch (channel.Site)
            {
                case SiteType.YouTube:

                    IEnumerable<IVideoItemPOCO> youres =
                        await commonFactory.CreateYouTubeSite().GetChannelItemsAsync(channel.ID, maxresult);
                    lst.AddRange(youres.Select(poco => vf.CreateVideoItem(poco)));

                    break;

                case SiteType.Tapochek:

                    IEnumerable<IVideoItemPOCO> tapres = await commonFactory.CreateTapochekSite().GetChannelItemsAsync(channel, maxresult);
                    lst.AddRange(tapres.Select(poco => vf.CreateVideoItem(poco)));

                    break;

                default:
                    throw new Exception(channel.SiteAdress + " is not implemented yet");
            }

            return lst;
        }

        public async Task<IChannel> GetChannelNetAsync(string channelID, SiteType site)
        {
            IChannelPOCO poco = null;
            try
            {
                switch (site)
                {
                    case SiteType.YouTube:
                        poco = await commonFactory.CreateYouTubeSite().GetChannelFullNetAsync(channelID);
                        break;
                    case SiteType.RuTracker:
                        poco = await commonFactory.CreateRutrackerSite().GetChannelNetAsync(channelID);
                        break;
                    case SiteType.Tapochek:
                        poco = await commonFactory.CreateTapochekSite().GetChannelNetAsync(channelID);
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

        public async Task<int> GetChannelPlaylistCountDbAsync(string id)
        {
            try
            {
                return await sql.GetChannelPlaylistCountDbAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IPlaylist>> GetChannelPlaylistsAsync(string channelID)
        {
            var lst = new List<IPlaylist>();
            try
            {
                IEnumerable<IPlaylistPOCO> fbres = await sql.GetChannelPlaylistAsync(channelID);
                lst.AddRange(fbres.Select(poco => pf.CreatePlaylist(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IPlaylist>> GetChannelPlaylistsNetAsync(string channelID)
        {
            var lst = new List<IPlaylist>();
            try
            {
                IEnumerable<IPlaylistPOCO> fbres = await YouTubeSite.GetChannelPlaylistsNetAsync(channelID);
                lst.AddRange(fbres.Select(poco => pf.CreatePlaylist(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<ITag>> GetChannelTagsAsync(string id)
        {
            TagFactory tf = commonFactory.CreateTagFactory();
            var lst = new List<ITag>();

            try
            {
                IEnumerable<ITagPOCO> fbres = await sql.GetChannelTagsAsync(id);
                lst.AddRange(fbres.Select(poco => tf.CreateTag(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IChannel>> GetRelatedChannelNetAsync(IChannel channel)
        {
            IEnumerable<IChannelPOCO> related = null;
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

        public async Task InsertChannelAsync(IChannel channel)
        {
            try
            {
                await sql.InsertChannelFullAsync(channel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertChannelItemsAsync(IChannel channel)
        {
            try
            {
                await sql.InsertChannelItemsAsync(channel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertChannelTagAsync(string channelid, string tag)
        {
            try
            {
                await sql.InsertChannelTagsAsync(channelid, tag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task RenameChannelAsync(string parentID, string newName)
        {
            try
            {
                await sql.RenameChannelAsync(parentID, newName);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void StoreCookies(string site, CookieContainer cookies)
        {
            try
            {
                sql.StoreCookies(site, cookies);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task SyncChannelAsync(IChannel channel)
        {
            channel.IsInWork = true;

            if (channel is YouChannel)
            {
                IPlaylist uploads = pf.CreatePlaylist(await sql.GetUploadPlaylistAsync(channel.ID));
                if (uploads == null)
                {
                    // old db
                    string oldname = channel.Title;
                    int oldcount = await sql.GetChannelItemsCountDbAsync(channel.ID);
                    IChannelPOCO newchannel = await you.GetChannelFullNetAsync(channel.ID);
                    channel.ChannelItems.Clear();
                    channel.ChannelPlaylists.Clear();
                    foreach (IVideoItemPOCO poco in newchannel.Items)
                    {
                        channel.ChannelItems.Add(vf.CreateVideoItem(poco));
                    }
                    foreach (IPlaylistPOCO poco in newchannel.Playlists)
                    {
                        channel.ChannelPlaylists.Add(pf.CreatePlaylist(poco));
                    }
                    channel.ChannelItemsCount = newchannel.Items.Count;

                    if (channel.Title != oldname)
                    {
                        channel.Title = oldname;
                    }

                    channel.CountNew = channel.ChannelItems.Count - oldcount;
                    await DeleteChannelAsync(channel.ID);
                    await InsertChannelAsync(channel);
                    return;
                }

                uploads.PlItems = (await sql.GetPlaylistItemsIdsListDbAsync(uploads.ID)).ToList();

                //IPlaylist uploads = pf.CreatePlaylist();
                //uploads.ID = channel.ID.Replace("UC", "UU");
                //uploads.PlItems = (await GetChannelItemsIdsListDbAsync(channel.ID)).ToList();

                int newCount = await YouTubeSite.GetPlaylistItemsCountNetAsync(uploads.ID);
                if (newCount == uploads.PlItems.Count)
                {
                    return;
                }
                if (newCount > uploads.PlItems.Count)
                {
                    int nc = newCount - uploads.PlItems.Count + 3; // с запасом
                    IEnumerable<string> newItems = await YouTubeSite.GetPlaylistItemsIdsListNetAsync(uploads.ID, nc);
                    List<string> trueids = newItems.Where(id => !uploads.PlItems.Contains(id)).ToList();
                    if (trueids.Any())
                    {
                        IEnumerable<List<string>> tchanks = CommonExtensions.SplitList(trueids);
                        foreach (List<string> list in tchanks)
                        {
                            IEnumerable<IVideoItemPOCO> res = await you.GetVideosListByIdsAsync(list); // получим скопом
                            foreach (IVideoItemPOCO poco in res)
                            {
                                IVideoItem vi = vf.CreateVideoItem(poco);
                                channel.AddNewItem(vi, true);
                                await vi.InsertItemAsync();
                                await uploads.UpdatePlaylistAsync(vi.ID);
                                channel.ChannelItemsCount = channel.ChannelItems.Count;
                            }
                        }
                    }
                }
                else
                {
                    // в инете меньше чем у нас
                }

                channel.IsInWork = false;
            }
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
        public async Task SyncChannelPlaylistsAsync(YouChannel channel)
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
                        IVideoItem item = await vf.GetVideoItemNetAsync(id, channel.Site);
                        if (item.ParentID != channel.ID)
                        {
                            continue;
                        }

                        channel.AddNewItem(item, true);
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
