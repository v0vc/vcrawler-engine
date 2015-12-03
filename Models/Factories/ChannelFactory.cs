// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Data;
using Extensions;
using Interfaces.API;
using Interfaces.Enums;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO.Channels;

namespace Models.Factories
{
    public class ChannelFactory : IChannelFactory
    {
        #region Static and Readonly Fields

        private readonly ICommonFactory _c;

        #endregion

        #region Constructors

        public ChannelFactory(ICommonFactory c)
        {
            _c = c;
        }

        #endregion

        #region Methods

        public async Task DeleteChannelAsync(string channelID)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.DeleteChannelAsync(channelID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteChannelTagAsync(string channelid, string tag)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.DeleteChannelTagsAsync(channelid, tag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void FillChannelCookieDb(IChannel channel)
        {
            ISqLiteDatabase cf = _c.CreateSqLiteDatabase();

            try
            {
                channel.ChannelCookies = cf.ReadCookies(channel.SiteAdress);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task FillChannelCookieNetAsync(YouChannel channel)
        {
            switch (channel.Site)
            {
                case SiteType.Tapochek:

                    ITapochekSite fb = _c.CreateTapochekSite();
                    channel.ChannelCookies = await fb.GetCookieNetAsync(channel);

                    break;

                default:
                    throw new Exception(channel + " is not implemented yet");
            }
        }

        public async Task FillChannelDescriptionAsync(IChannel channel)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            channel.SubTitle = await fb.GetChannelDescriptionAsync(channel.ID);
        }

        public async Task FillChannelItemsFromDbAsync(IChannel channel, string dir, int count, int offset)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            IVideoItemFactory vf = _c.CreateVideoItemFactory();
            try
            {
                channel.ChannelItemsCount = await fb.GetChannelItemsCountDbAsync(channel.ID);

                List<string> lst = (await fb.GetChannelItemsIdListDbAsync(channel.ID, count, offset)).ToList();

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
            ICredFactory cf = _c.CreateCredFactory();
            try
            {
                return await cf.GetCredDbAsync(channel.Site);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> GetChannelItemsCountDbAsync(string channelID)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                return await fb.GetChannelItemsCountDbAsync(channelID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> GetChannelItemsCountNetAsync(string channelID)
        {
            IYouTubeSite fb = _c.CreateYouTubeSite();
            try
            {
                return await fb.GetChannelItemsCountNetAsync(channelID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IVideoItem>> GetChannelItemsDbAsync(string channelID, int count, int offset)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            IVideoItemFactory vf = _c.CreateVideoItemFactory();
            var lst = new List<IVideoItem>();

            try
            {
                IEnumerable<IVideoItemPOCO> fbres = await fb.GetChannelItemsAsync(channelID, count, offset);
                lst.AddRange(fbres.Select(vf.CreateVideoItem));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<string>> GetChannelItemsIdsListDbAsync(string channelID)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                return await fb.GetChannelItemsIdListDbAsync(channelID, 0, 0);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<string>> GetChannelItemsIdsListNetAsync(string channelID, int maxResult)
        {
            IYouTubeSite fb = _c.CreateYouTubeSite();
            try
            {
                return await fb.GetChannelItemsIdsListNetAsync(channelID, maxResult);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IVideoItem>> GetChannelItemsNetAsync(YouChannel channel, int maxresult)
        {
            IVideoItemFactory vf = _c.CreateVideoItemFactory();
            var lst = new List<IVideoItem>();
            switch (channel.Site)
            {
                case SiteType.YouTube:

                    IEnumerable<IVideoItemPOCO> youres = await _c.CreateYouTubeSite().GetChannelItemsAsync(channel.ID, maxresult);
                    lst.AddRange(youres.Select(vf.CreateVideoItem));

                    break;

                case SiteType.Tapochek:

                    IEnumerable<IVideoItemPOCO> tapres = await _c.CreateTapochekSite().GetChannelItemsAsync(channel, maxresult);
                    lst.AddRange(tapres.Select(vf.CreateVideoItem));

                    break;

                default:
                    throw new Exception(channel.SiteAdress + " is not implemented yet");
            }

            return lst;
        }

        public async Task<int> GetChannelPlaylistCountDbAsync(string id)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                return await fb.GetChannelPlaylistCountDbAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IPlaylist>> GetChannelPlaylistsAsync(string channelID)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            IPlaylistFactory pf = _c.CreatePlaylistFactory();
            var lst = new List<IPlaylist>();

            try
            {
                IEnumerable<IPlaylistPOCO> fbres = await fb.GetChannelPlaylistAsync(channelID);
                lst.AddRange(fbres.Select(pf.CreatePlaylist));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IPlaylist>> GetChannelPlaylistsNetAsync(string channelID)
        {
            IYouTubeSite fb = _c.CreateYouTubeSite();
            IPlaylistFactory pf = _c.CreatePlaylistFactory();
            var lst = new List<IPlaylist>();
            try
            {
                IEnumerable<IPlaylistPOCO> fbres = await fb.GetChannelPlaylistNetAsync(channelID);
                lst.AddRange(fbres.Select(pf.CreatePlaylist));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<ITag>> GetChannelTagsAsync(string id)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            ITagFactory tf = _c.CreateTagFactory();
            var lst = new List<ITag>();

            try
            {
                IEnumerable<ITagPOCO> fbres = await fb.GetChannelTagsAsync(id);
                lst.AddRange(fbres.Select(tf.CreateTag));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IVideoItem>> GetPopularItemsNetAsync(string regionID, int maxresult)
        {
            IYouTubeSite fb = _c.CreateYouTubeSite();
            IVideoItemFactory vf = _c.CreateVideoItemFactory();
            var lst = new List<IVideoItem>();

            try
            {
                IEnumerable<IVideoItemPOCO> fbres = await fb.GetPopularItemsAsync(regionID, maxresult);
                lst.AddRange(fbres.Select(vf.CreateVideoItem));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IChannel>> GetRelatedChannelNetAsync(string id, SiteType site)
        {
            IEnumerable<IChannelPOCO> related;
            switch (site)
            {
                case SiteType.YouTube:
                    related = await _c.CreateYouTubeSite().GetRelatedChannelsByIdAsync(id);
                    break;

                default:
                    related = new List<IChannelPOCO>();
                    break;
            }

            return related.Select(CreateChannel);
        }

        public async Task InsertChannelAsync(IChannel channel)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.InsertChannelAsync(channel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertChannelItemsAsync(IChannel channel)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.InsertChannelItemsAsync(channel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertChannelTagAsync(string channelid, string tag)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.InsertChannelTagsAsync(channelid, tag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task RenameChannelAsync(string parentID, string newName)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.RenameChannelAsync(parentID, newName);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IVideoItem>> SearchItemsNetAsync(string key, string regionID, int maxresult)
        {
            IYouTubeSite fb = _c.CreateYouTubeSite();
            IVideoItemFactory vf = _c.CreateVideoItemFactory();
            var lst = new List<IVideoItem>();

            try
            {
                IEnumerable<IVideoItemPOCO> fbres = await fb.SearchItemsAsync(key, regionID, maxresult);
                lst.AddRange(fbres.Select(vf.CreateVideoItem));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void StoreCookies(string site, CookieContainer cookies)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                fb.StoreCookies(site, cookies);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task SyncChannelAsync(YouChannel channel, bool isSyncPls)
        {
            channel.IsInWork = true;

            // получаем количество записей в базе
            // var dbCount = await GetChannelItemsCountDbAsync(YouChannel.ID);
            List<string> idsdb = (await GetChannelItemsIdsListDbAsync(channel.ID)).ToList();

            // получаем количество записей на канале
            // var lsids = await YouChannel.GetChannelItemsIdsListNetAsync(0);
            // var netCount = lsids.Count;
            int netCount = await GetChannelItemsCountNetAsync(channel.ID);

            if (netCount > idsdb.Count)
            {
                int nc = netCount - idsdb.Count + 1; // с запасом :)
                List<string> lsidNet = (await channel.GetChannelItemsIdsListNetAsync(nc)).ToList();

                if (lsidNet.Count != idsdb.Count && lsidNet.Any())
                {
                    lsidNet.Reverse();
                    List<string> trueids = lsidNet.Where(id => !idsdb.Contains(id)).ToList(); // id которых нет
                    if (trueids.Any())
                    {
                        IVideoItemFactory vf = _c.CreateVideoItemFactory();
                        IYouTubeSite you = _c.CreateYouTubeSite();

                        IEnumerable<List<string>> tchanks = CommonExtensions.SplitList(trueids); // бьем на чанки - минимизируем запросы

                        foreach (List<string> list in tchanks)
                        {
                            IEnumerable<IVideoItemPOCO> res = await you.GetVideosListByIdsAsync(list); // получим скопом

                            foreach (IVideoItemPOCO poco in res)
                            {
                                IVideoItem vi = vf.CreateVideoItem(poco);
                                channel.AddNewItem(vi, true);
                                await vi.InsertItemAsync();
                            }
                        }
                    }
                }
            }

            if (isSyncPls)
            {
                List<IPlaylist> dbpls = (await channel.GetChannelPlaylistsDbAsync()).ToList(); // получаем все плэйлисты из базы

                List<IPlaylist> pls = (await channel.GetChannelPlaylistsNetAsync()).ToList(); // получаем все плэйлисты из сети

                // в сети изменилось количество плэйлистов - тупо все удалим и запишем заново
                if (dbpls.Count != pls.Count)
                {
                    foreach (IPlaylist pl in dbpls)
                    {
                        await pl.DeletePlaylistAsync();
                    }

                    foreach (IPlaylist pl in pls)
                    {
                        await pl.InsertPlaylistAsync();

                        IEnumerable<string> plv = await pl.GetPlaylistItemsIdsListNetAsync();

                        foreach (string id in plv)
                        {
                            if (channel.ChannelItems.Select(x => x.ID).Contains(id))
                            {
                                await pl.UpdatePlaylistAsync(id);
                            }
                        }
                    }
                }
                else
                {
                    // количество плэйлистов в базе и в сети одинаково - посмотрим на содержимое
                    foreach (IPlaylist pl in pls)
                    {
                        // получим количество видюх плейлиста в сети
                        List<string> plv = (await pl.GetPlaylistItemsIdsListNetAsync()).ToList();

                        // получим количество видюх плэйлиста в базе
                        IEnumerable<string> plvdb = await pl.GetPlaylistItemsIdsListDbAsync();

                        // если равно - считаем что содержимое плейлиста не изменилось (не факт конечно, но да пох)
                        if (plv.Count == plvdb.Count())
                        {
                            continue;
                        }

                        // изменилось содержимое плэйлиста - тупо удалим его (бд - каскад) и запишем с новыми данными
                        await pl.DeletePlaylistAsync();

                        await pl.InsertPlaylistAsync(); // запишем

                        foreach (string id in plv)
                        {
                            // обновим
                            if (channel.ChannelItems.Select(x => x.ID).Contains(id))
                            {
                                await pl.UpdatePlaylistAsync(id);
                            }
                        }
                    }
                }
            }
            channel.IsInWork = false;
        }

        public async Task SyncChannelPlaylistsAsync(YouChannel channel)
        {
            List<IPlaylist> pls = (await channel.GetChannelPlaylistsNetAsync()).ToList();

            if (pls.Any())
            {
                await channel.DeleteChannelPlaylistsAsync();
                channel.ChannelPlaylists.Clear();
            }

            IVideoItemFactory vf = _c.CreateVideoItemFactory();

            foreach (IPlaylist playlist in pls)
            {
                await playlist.InsertPlaylistAsync();

                IEnumerable<string> plv = await playlist.GetPlaylistItemsIdsListNetAsync();

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

        #region IChannelFactory Members

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
            var channel = new YouChannel(this)
            {
                ID = poco.ID, 
                Title = poco.Title, 
                SubTitle = poco.SubTitle, // .WordWrap(80);
                Thumbnail = poco.Thumbnail, 
                SiteAdress = poco.Site, 
                ChannelItems = new ObservableCollection<IVideoItem>(), 
                ChannelPlaylists = new ObservableCollection<IPlaylist>(), 
                ChannelTags = new ObservableCollection<ITag>(), 
                ChannelCookies = new CookieContainer(), 
                Site = CommonExtensions.GetSiteType(poco.Site)
            };
            channel.ChannelItemsCollectionView = CollectionViewSource.GetDefaultView(channel.ChannelItems);
            return channel;
        }

        public async Task<IChannel> GetChannelDbAsync(string channelID)
        {
            // var fb = ServiceLocator.SqLiteDatabase;
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                IChannelPOCO poco = await fb.GetChannelAsync(channelID);
                IChannel channel = CreateChannel(poco);
                return channel;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> GetChannelIdByUserNameNetAsync(string username)
        {
            IYouTubeSite fb = _c.CreateYouTubeSite();
            try
            {
                return await fb.GetChannelIdByUserNameNetAsync(username);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IChannel> GetChannelNetAsync(string channelID, SiteType site)
        {
            IChannelPOCO poco = null;
            try
            {
                switch (site)
                {
                    case SiteType.YouTube:
                        poco = await _c.CreateYouTubeSite().GetChannelNetAsync(channelID);
                        break;
                    case SiteType.RuTracker:
                        poco = await _c.CreateRutrackerSite().GetChannelNetAsync(channelID);
                        break;
                    case SiteType.Tapochek:
                        poco = await _c.CreateTapochekSite().GetChannelNetAsync(channelID);
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

        #endregion

        public async Task DeleteChannelPlaylistsAsync(string channelID)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                IEnumerable<string> lst = await fb.GetChannelsPlaylistsIdsListDbAsync(channelID);
                foreach (string id in lst)
                {
                    await fb.DeletePlaylistAsync(id);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
