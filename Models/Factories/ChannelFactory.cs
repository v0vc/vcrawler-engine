using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO;

namespace Models.Factories
{
    public class ChannelFactory : IChannelFactory
    {
        private readonly ICommonFactory _c;

        //private readonly ISqLiteDatabase _db;

        //private readonly IYouTubeSite _youTubeSite;

        public ChannelFactory(ICommonFactory c)
        {
            _c = c;
            //_db = c.CreateSqLiteDatabase();
            //_youTubeSite = c.CreateYouTubeSite();
        }

        public IChannel CreateChannel()
        {
            return new Channel(this);
        }

        public async Task<IChannel> GetChannelDbAsync(string channelID)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var poco = await fb.GetChannelAsync(channelID);
                return new Channel(poco, _c.CreateChannelFactory());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IChannel> GetChannelNetAsync(string channelID)
        {
            var fb = _c.CreateYouTubeSite();
            //var fb = ServiceLocator.YouTubeSiteApiV2;
            try
            {
                var poco = await fb.GetChannelNetAsync(channelID);
                return new Channel(poco, _c.CreateChannelFactory());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IVideoItem>> GetChannelItemsDbAsync(string channelID)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.GetChannelItemsAsync(channelID);
                foreach (IVideoItemPOCO poco in fbres)
                {
                    var vi = new VideoItem(poco, _c.CreateVideoItemFactory());
                    vi.IsHasLocalFile = false;
                    //vi.ProgressBarVisibility = Visibility.Hidden;
                    lst.Add(vi);
                }
                //lst.AddRange(fbres.Select(item => new VideoItem(item, _c.CreateVideoItemFactory())));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertChannelAsync(IChannel channel)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.InsertChannelAsync(channel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteChannelAsync(string channelID)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.DeleteChannelAsync(channelID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task RenameChannelAsync(string parentID, string newName)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.RenameChannelAsync(parentID, newName);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        public async Task InsertChannelItemsAsync(IChannel channel)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.InsertChannelItemsAsync(channel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IVideoItem>> GetChannelItemsNetAsync(string channelID, int maxresult)
        {
            var fb = _c.CreateYouTubeSite();
            //var fb = ServiceLocator.YouTubeSiteApiV2;
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.GetChannelItemsAsync(channelID, maxresult);
                lst.AddRange(fbres.Select(item => new VideoItem(item, _c.CreateVideoItemFactory())));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IVideoItem>> GetPopularItemsNetAsync(string regionID, int maxresult)
        {
            var fb = _c.CreateYouTubeSite();
            //var fb = ServiceLocator.YouTubeSiteApiV2;
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.GetPopularItemsAsync(regionID, maxresult);
                lst.AddRange(fbres.Select(item => new VideoItem(item, _c.CreateVideoItemFactory())));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IVideoItem>> SearchItemsNetAsync(string key, int maxresult)
        {
            var fb = _c.CreateYouTubeSite();
            //var fb = ServiceLocator.YouTubeSiteApiV2;
            try
            {
                var lst = new List<IVideoItem>();
                var fbres = await fb.SearchItemsAsync(key, maxresult);
                lst.AddRange(fbres.Select(item => new VideoItem(item, _c.CreateVideoItemFactory())));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IPlaylist>> GetChannelPlaylistsNetAsync(string channelID)
        {
            var fb = _c.CreateYouTubeSite();
            //var fb = ServiceLocator.YouTubeSiteApiV2;
            try
            {
                var lst = new List<IPlaylist>();
                var fbres = await fb.GetChannelPlaylistNetAsync(channelID);
                lst.AddRange(fbres.Select(item => new Playlist(item, _c.CreatePlaylistFactory())));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IPlaylist>> GetChannelPlaylistsAsync(string channelID)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var lst = new List<IPlaylist>();
                var fbres = await fb.GetChannelPlaylistAsync(channelID);
                lst.AddRange(fbres.Select(item => new Playlist(item, _c.CreatePlaylistFactory())));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }            
        }

        public async Task<List<ITag>> GetChannelTagsAsync(string id)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var lst = new List<ITag>();
                var fbres = await fb.GetChannelTagsAsync(id);
                lst.AddRange(fbres.Select(item => new Tag(item, _c.CreateTagFactory())));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertChannelTagAsync(string channelid, string tag)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.InsertChannelTagsAsync(channelid, tag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteChannelTagAsync(string channelid, string tag)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.DeleteChannelTagsAsync(channelid, tag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task SyncChannelAsync(IChannel channel, bool isSyncPls)
        {
            //получаем количество записей в базе
            var dbCount = await GetChannelItemsCountDbAsync(channel.ID);

            //получаем количество записей на канале
            //var lsid = await channel.GetChannelItemsIdsListNetAsync(0);
            //var netCount = lsid.Count;

            var netCount = await GetChannelItemsCountNetAsync(channel.ID);

            if (netCount > dbCount)
            {
                var nc = netCount - dbCount;

                var lsid = await channel.GetChannelItemsIdsListNetAsync(nc);

                if (lsid.Count != dbCount && lsid.Any())
                {
                    var vf = _c.CreateVideoItemFactory();

                    if (!channel.ChannelItems.Any())
                        await channel.FillChannelItemsDbAsync();

                    lsid.Reverse();

                    foreach (string id in lsid)
                    {
                        if (!channel.ChannelItems.Select(x => x.ID).Contains(id))
                        {
                            var item = await vf.GetVideoItemNetAsync(id);
                            item.IsNewItem = true;
                            item.IsShowRow = true;
                            //channel.ChannelItems.Add(item);
                            channel.ChannelItems.Insert(0, item);
                            channel.CountNew += 1;
                            await item.InsertItemAsync();
                        }
                    }
                }
            }

            if (isSyncPls)
            {
                var dbpls = await channel.GetChannelPlaylistsAsync(); //получаем все плэйлисты из базы

                var pls = await channel.GetChannelPlaylistsNetAsync(); //получаем все плэйлисты из сети

                //в сети изменилось количество плэйлистов - тупо все удалим и запишем заново
                if (dbpls.Count != pls.Count)
                {
                    foreach (var pl in dbpls)
                    {
                        await pl.DeletePlaylistAsync();
                    }

                    foreach (var pl in pls)
                    {
                        await pl.InsertPlaylistAsync();

                        var plv = await pl.GetPlaylistItemsIdsListNetAsync();

                        foreach (string id in plv)
                        {
                            if (channel.ChannelItems.Select(x => x.ID).Contains(id))
                                await pl.UpdatePlaylistAsync(id);
                        }
                    }
                }
                else //количество плэйлистов в базе и в сети одинаково - посмотрим на содержимое
                {
                    foreach (var pl in pls)
                    {
                        //if (pl.ID == "PLyIeuhp1AcC0EXyiaXRNZxdLV1dXyzuAG")

                        //получим количество видюх плейлиста в сети
                        var plv = await pl.GetPlaylistItemsIdsListNetAsync();

                        //получим количество видюх плэйлиста в базе
                        var plvdb = await pl.GetPlaylistItemsIdsListDbAsync();

                        //если равно - считаем что содержимое плейлиста не изменилось (не факт конечно, но да пох)
                        if (plv.Count == plvdb.Count)
                            continue;

                        //изменилось содержимое плэйлиста - тупо удалим его (бд - каскад) и запишем с новыми данными
                        await pl.DeletePlaylistAsync();

                        await pl.InsertPlaylistAsync(); //запишем

                        foreach (string id in plv) //обновим
                        {
                            if (channel.ChannelItems.Select(x => x.ID).Contains(id))
                            {
                                try
                                {
                                    await pl.UpdatePlaylistAsync(id);
                                }
                                catch (Exception)
                                {
                                    Debug.WriteLine(id);
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task<int> GetChannelItemsCountDbAsync(string channelID)
        {
            var fb = _c.CreateSqLiteDatabase();
            //var fb = ServiceLocator.SqLiteDatabase;
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
            var fb = _c.CreateYouTubeSite();
            //var fb = ServiceLocator.YouTubeSiteApiV2;
            try
            {
                return await fb.GetChannelItemsCountNetAsync(channelID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<string>> GetChannelItemsIdsListNetAsync(string channelID, int maxResult)
        {
            var fb = _c.CreateYouTubeSite();
            try
            {
                return await fb.GetChannelItemsIdsListNetAsync(channelID, maxResult);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<string>> GetChannelItemsIdsListDbAsync(string channelID)
        {
            var fb = _c.CreateSqLiteDatabase();
            try
            {
                return await fb.GetChannelItemsIdListDbAsync(channelID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> GetChannelIdByUserNameNetAsync(string username)
        {
            var fb = _c.CreateYouTubeSite();
            try
            {
                return await fb.GetChannelIdByUserNameNetAsync(username);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task FillChannelItemsFromDbAsync(IChannel channel)
        {
            var fb = _c.CreateSqLiteDatabase();
            var vf = _c.CreateVideoItemFactory();
            //var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var lst = await fb.GetChannelItemsIdListDbAsync(channel.ID);
                if (lst.Any())
                {
                    foreach (string id in lst)
                    {
                        var vid = await vf.GetVideoItemDbAsync(id);
                        vid.IsShowRow = true;
                        channel.ChannelItems.Add(vid);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
