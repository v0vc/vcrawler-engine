// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DataAPI.Database;
using Extensions.Helpers;
using Interfaces.Enums;
using Interfaces.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models.Factories;

namespace TestAPI
{
    [TestClass]
    public class SqLiteDataBaseTest
    {
        #region Static and Readonly Fields

        private readonly SqLiteDatabase db;

        #endregion

        #region Constructors

        public SqLiteDataBaseTest()
        {
            db = CommonFactory.CreateSqLiteDatabase();
        }

        #endregion

        #region Static Methods

        private static async void FillTestChannel(IChannel ch, IVideoItem v1, IVideoItem v2, ICred cred)
        {
            ch.ChannelItems = new ObservableCollection<IVideoItem>();
            ch.ID = "testch";
            ch.Title = "тестовая канал, для отладки слоя бд";
            ch.SubTitle = "использутеся для отдладки :)";
            ch.Thumbnail = await SiteHelper.GetStreamFromUrl(null);
            ch.Site = cred.Site;
            ch.ChannelItems.Add(v1);
            ch.ChannelItems.Add(v2);
        }

        private static void FillTestCred(ICred cred)
        {
            cred.SiteAdress = "testsite.com";
            cred.Login = "testlogin";
            cred.Pass = "testpass";
            cred.Cookie = "cookie";
            cred.Expired = DateTime.Now;
            cred.Autorization = 0;
        }

        private static async void FillTestPl(IPlaylist pl, IChannel ch)
        {
            pl.ID = "testID";
            pl.Title = "Плейлист №1";
            pl.SubTitle = "test subtitle";
            pl.Thumbnail = await SiteHelper.GetStreamFromUrl(null);
            pl.ChannelId = ch.ID;
        }

        private static void FillTestSetting(ISetting setting)
        {
            setting.Key = "testsetting";
            setting.Value = "testvalue";
        }

        private static void FillTestTag(ITag tag)
        {
            tag.Title = "testag";
        }

        private static async void FillTestVideoItem(IVideoItem vi, SyncState state)
        {
            vi.ID = "vi";
            vi.ParentID = "testch";
            vi.Title = "отдельный итем";
            vi.Description = "для отладки";
            vi.ViewCount = 123;
            vi.Duration = 321;
            vi.Comments = 123;
            vi.Thumbnail = await SiteHelper.GetStreamFromUrl(null);
            vi.Timestamp = DateTime.Now;
            vi.SyncState = state;
        }

        #endregion

        #region Methods

        [TestMethod]
        public void TestCrudCredentials()
        {
            ICred cred = CredFactory.CreateCred();
            FillTestCred(cred);

            // DeleteCredAsync
            Task t = db.DeleteCredAsync(cred.SiteAdress);
            Assert.IsTrue(!t.IsFaulted);

            // InsertCredAsync
            t = db.InsertCredAsync(cred);
            Assert.IsTrue(!t.IsFaulted);

            // GetCredAsync
            t = db.GetCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);

            // UpdateLoginAsync
            t = db.UpdateLoginAsync(cred.SiteAdress, "newlogin");
            Assert.IsTrue(!t.IsFaulted);

            // UpdatePasswordAsync
            t = db.UpdatePasswordAsync(cred.SiteAdress, "newpassword");
            Assert.IsTrue(!t.IsFaulted);

            // UpdateAutorizationAsync
            t = db.UpdateAutorizationAsync(cred.SiteAdress, 1);
            Assert.IsTrue(!t.IsFaulted);

            // GetCredListAsync
            t = db.GetCredListAsync();
            Assert.IsTrue(!t.IsFaulted);

            // DeleteCredAsync
            t = db.DeleteCredAsync(cred.SiteAdress);
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public void TestCrudItems()
        {
            IVideoItem vi = VideoItemFactory.CreateVideoItem(SiteType.YouTube);
            FillTestVideoItem(vi, SyncState.Added);
            IVideoItem vi2 = VideoItemFactory.CreateVideoItem(SiteType.YouTube);
            FillTestVideoItem(vi2, SyncState.Notset);
            vi2.ID = "vi2";
            var lst = new List<IVideoItem> { vi, vi2 };
            ICred cred = CredFactory.CreateCred();
            FillTestCred(cred);
            IChannel ch = ChannelFactory.CreateChannel(SiteType.YouTube);
            FillTestChannel(ch, vi, vi2, cred);

            // DeleteCredAsync
            Task t = db.DeleteCredAsync(cred.SiteAdress);
            Assert.IsTrue(!t.IsFaulted);

            // InsertCredAsync
            t = db.InsertCredAsync(cred);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // InsertChannelAsync
            t = db.InsertChannelAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            // RenameChannelAsync
            t = db.RenameChannelAsync(ch.ID, "newname");
            Assert.IsTrue(!t.IsFaulted);

            // UpdateChannelNewCountAsync
            t = db.UpdateChannelNewCountAsync(ch.ID, 5);
            Assert.IsTrue(!t.IsFaulted);

            // GetChannelAsync
            t = db.GetChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // GetChannelsListAsync
            t = db.GetChannelsListAsync();
            Assert.IsTrue(!t.IsFaulted);

            // GetChannelItemsAsync
            t = db.GetChannelItemsAsync(ch.ID, 0, 0);
            Assert.IsTrue(!t.IsFaulted);

            // GetChannelItemsCountDbAsync
            t = db.GetChannelItemsCountDbAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // InsertChannelItemsAsync
            t = db.InsertChannelItemsAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // ITEMS

            // InsertChannelAsync
            t = db.InsertChannelAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            // InsertItemAsync
            t = db.InsertItemAsync(vi);
            Assert.IsTrue(!t.IsFaulted);

            // UpdateItemSyncState
            t = db.UpdateItemSyncState(vi.ID, SyncState.Deleted);
            Assert.IsTrue(!t.IsFaulted);

            // UpdateItemSyncState
            t = db.UpdateItemSyncState(lst, SyncState.Deleted);
            Assert.IsTrue(!t.IsFaulted);

            // GetVideoItemAsync
            t = db.GetVideoItemAsync(vi.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteItemAsync
            t = db.DeleteItemAsync(vi.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteCredAsync
            t = db.DeleteCredAsync(cred.SiteAdress);
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public void TestCrudPlaylists()
        {
            IVideoItem vi = VideoItemFactory.CreateVideoItem(SiteType.YouTube);
            FillTestVideoItem(vi, SyncState.Added);

            IVideoItem vi2 = VideoItemFactory.CreateVideoItem(SiteType.YouTube);
            FillTestVideoItem(vi2, SyncState.Deleted);
            vi2.ID = "vi2";

            ICred cred = CredFactory.CreateCred();
            FillTestCred(cred);

            IChannel ch = ChannelFactory.CreateChannel(SiteType.YouTube);
            FillTestChannel(ch, vi, vi2, cred);

            IPlaylist pl = PlaylistFactory.CreatePlaylist();
            FillTestPl(pl, ch);

            // DeleteCredAsync
            Task t = db.DeleteCredAsync(cred.SiteAdress);
            Assert.IsTrue(!t.IsFaulted);

            // InsertCredAsync
            t = db.InsertCredAsync(cred);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // InsertChannelItemsAsync
            t = db.InsertChannelItemsAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            // DeletePlaylistAsync
            t = db.DeletePlaylistAsync(pl.ID);
            Assert.IsTrue(!t.IsFaulted);

            // InsertPlaylistAsync
            t = db.InsertPlaylistAsync(pl);
            Assert.IsTrue(!t.IsFaulted);

            // GetPlaylistAsync
            t = db.GetPlaylistAsync(pl.ID);
            Assert.IsTrue(!t.IsFaulted);

            // GetChannelPlaylistAsync
            t = db.GetChannelPlaylistAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // UpdatePlaylistAsync
            t = db.UpdatePlaylistAsync(pl.ID, vi.ID, ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // GetPlaylistItemsAsync
            t = db.GetPlaylistItemsAsync(pl.ID, ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeletePlaylistAsync
            t = db.DeletePlaylistAsync(pl.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteCredAsync
            t = db.DeleteCredAsync(cred.SiteAdress);
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public void TestCrudSettings()
        {
            ISetting setting = SettingFactory.CreateSetting();
            FillTestSetting(setting);

            // DeleteSettingAsync
            Task t = db.DeleteSettingAsync(setting.Key);
            Assert.IsTrue(!t.IsFaulted);

            // InsertSettingAsync
            t = db.InsertSettingAsync(setting);
            Assert.IsTrue(!t.IsFaulted);

            // UpdateSettingAsync
            t = db.UpdateSettingAsync(setting.Key, "newvalue");
            Assert.IsTrue(!t.IsFaulted);

            // GetSettingAsync
            t = db.GetSettingAsync(setting.Key);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteSettingAsync
            t = db.DeleteSettingAsync(setting.Key);
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public void TestCrudTags()
        {
            ITag tag = TagFactory.CreateTag();
            FillTestTag(tag);

            // DeleteTagAsync
            Task t = db.DeleteTagAsync(tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            // InsertTagAsync
            t = db.InsertTagAsync(tag);
            Assert.IsTrue(!t.IsFaulted);

            IVideoItem vi = VideoItemFactory.CreateVideoItem(SiteType.YouTube);
            FillTestVideoItem(vi, SyncState.Notset);

            IVideoItem vi2 = VideoItemFactory.CreateVideoItem(SiteType.YouTube);
            FillTestVideoItem(vi2, SyncState.Deleted);
            vi2.ID = "vi2";

            ICred cred = CredFactory.CreateCred();
            FillTestCred(cred);

            IChannel ch = ChannelFactory.CreateChannel(SiteType.YouTube);
            FillTestChannel(ch, vi, vi2, cred);

            // DeleteCredAsync
            t = db.DeleteCredAsync(cred.SiteAdress);
            Assert.IsTrue(!t.IsFaulted);

            // InsertCredAsync
            t = db.InsertCredAsync(cred);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // InsertChannelAsync
            t = db.InsertChannelAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            // InsertChannelTagsAsync
            t = db.InsertChannelTagsAsync(ch.ID, tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            // InsertChannelTagsAsync
            t = db.InsertChannelTagsAsync(ch.ID, tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            // GetChannelTagsAsync
            t = db.GetChannelTagsAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // GetChannelsByTagAsync
            t = db.GetChannelsByTagAsync(tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelTagsAsync
            t = db.DeleteChannelTagsAsync(ch.ID, tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteTagAsync
            t = db.DeleteTagAsync(tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteCredAsync
            t = db.DeleteCredAsync(cred.SiteAdress);
            Assert.IsTrue(!t.IsFaulted);
        }

        #endregion
    }
}
