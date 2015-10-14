// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using Interfaces.API;
using Interfaces.Factories;
using Interfaces.Models;
using IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace TestAPI
{
    [TestClass]
    public class SqLiteDataBaseTest
    {
        #region Constants

        private const string TestPhoto = "https://yt3.ggpht.com/-qMfsaQGvdSQ/AAAAAAAAAAI/AAAAAAAAAAA/NLFnUAWeiaM/s88-c-k-no/photo.jpg";

        #endregion

        #region Static and Readonly Fields

        private readonly IChannelFactory _cf;
        private readonly ICredFactory _crf;
        private readonly ISqLiteDatabase _db;
        private readonly ICommonFactory _factory;
        private readonly IPlaylistFactory _pf;
        private readonly ISettingFactory _sf;
        private readonly ITagFactory _tf;
        private readonly IVideoItemFactory _vf;

        #endregion

        #region Constructors

        public SqLiteDataBaseTest()
        {
            _factory = Container.Kernel.Get<ICommonFactory>();
            _vf = _factory.CreateVideoItemFactory();
            _db = _factory.CreateSqLiteDatabase();
            _cf = _factory.CreateChannelFactory();
            _crf = _factory.CreateCredFactory();
            _pf = _factory.CreatePlaylistFactory();
            _tf = _factory.CreateTagFactory();
            _sf = _factory.CreateSettingFactory();
        }

        #endregion

        #region Static Methods

        private static void FillTestChannel(IChannel ch, IVideoItem v1, IVideoItem v2, ICred cred)
        {
            ch.ChannelItems = new ObservableCollection<IVideoItem>();
            ch.ID = "testch";
            ch.Title = "тестовая канал, для отладки слоя бд";
            ch.SubTitle = "использутеся для отдладки :)";
            ch.Thumbnail = GetStreamFromUrl(TestPhoto);
            ch.SiteAdress = cred.Site;
            ch.ChannelItems.Add(v1);
            ch.ChannelItems.Add(v2);
        }

        private static void FillTestCred(ICred cred)
        {
            cred.Site = "testsite.com";
            cred.Login = "testlogin";
            cred.Pass = "testpass";
            cred.Cookie = "cookie";
            cred.Expired = DateTime.Now;
            cred.Autorization = 0;
        }

        private static void FillTestPl(IPlaylist pl, IChannel ch)
        {
            pl.ID = "testID";
            pl.Title = "Плейлист №1";
            pl.SubTitle = "test subtitle";
            pl.Thumbnail = GetStreamFromUrl(TestPhoto);
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

        private static void FillTestVideoItem(IVideoItem vi)
        {
            vi.ID = "vi";
            vi.ParentID = "testch";
            vi.Title = "отдельный итем";
            vi.Description = "для отладки";
            vi.ViewCount = 123;
            vi.Duration = 321;
            vi.Comments = 123;
            vi.Thumbnail = GetStreamFromUrl("https://i.ytimg.com/vi/29vzpOxZ_ys/1.jpg");
            vi.Timestamp = DateTime.Now;
        }

        private static byte[] GetStreamFromUrl(string url)
        {
            byte[] imageData;

            using (var wc = new WebClient())
            {
                imageData = wc.DownloadData(url);
            }

            return imageData;
        }

        #endregion

        #region Methods

        [TestMethod]
        public void TestCrudCredentials()
        {
            ISqLiteDatabase db = _factory.CreateSqLiteDatabase();

            ICred cred = _crf.CreateCred();
            FillTestCred(cred);

            // DeleteCredAsync
            Task t = db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);

            // InsertCredAsync
            t = db.InsertCredAsync(cred);
            Assert.IsTrue(!t.IsFaulted);

            // GetCredAsync
            t = db.GetCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);

            // UpdateLoginAsync
            t = db.UpdateLoginAsync(cred.Site, "newlogin");
            Assert.IsTrue(!t.IsFaulted);

            // UpdatePasswordAsync
            t = db.UpdatePasswordAsync(cred.Site, "newpassword");
            Assert.IsTrue(!t.IsFaulted);

            // UpdateAutorizationAsync
            t = db.UpdateAutorizationAsync(cred.Site, 1);
            Assert.IsTrue(!t.IsFaulted);

            // GetCredListAsync
            t = db.GetCredListAsync();
            Assert.IsTrue(!t.IsFaulted);

            // DeleteCredAsync
            t = db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public void TestCrudItems()
        {
            IVideoItem vi = _vf.CreateVideoItem();
            FillTestVideoItem(vi);
            IVideoItem vi2 = _vf.CreateVideoItem();
            FillTestVideoItem(vi2);
            vi2.ID = "vi2";
            ICred cred = _crf.CreateCred();
            FillTestCred(cred);
            IChannel ch = _cf.CreateChannel();
            FillTestChannel(ch, vi, vi2, cred);

            // DeleteCredAsync
            Task t = _db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);

            // InsertCredAsync
            t = _db.InsertCredAsync(cred);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // InsertChannelAsync
            t = _db.InsertChannelAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            // RenameChannelAsync
            t = _db.RenameChannelAsync(ch.ID, "newname");
            Assert.IsTrue(!t.IsFaulted);

            // GetChannelAsync
            t = _db.GetChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // GetChannelsListAsync
            t = _db.GetChannelsListAsync();
            Assert.IsTrue(!t.IsFaulted);

            // GetChannelItemsAsync
            t = _db.GetChannelItemsAsync(ch.ID, 0, 0);
            Assert.IsTrue(!t.IsFaulted);

            // GetChannelItemsCountDbAsync
            t = _db.GetChannelItemsCountDbAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // InsertChannelItemsAsync
            t = _db.InsertChannelItemsAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // ITEMS

            // InsertChannelAsync
            t = _db.InsertChannelAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            // InsertItemAsync
            t = _db.InsertItemAsync(vi);
            Assert.IsTrue(!t.IsFaulted);

            // GetVideoItemAsync
            t = _db.GetVideoItemAsync(vi.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteItemAsync
            t = _db.DeleteItemAsync(vi.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteCredAsync
            t = _db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public void TestCrudPlaylists()
        {
            IVideoItem vi = _vf.CreateVideoItem();
            FillTestVideoItem(vi);

            IVideoItem vi2 = _vf.CreateVideoItem();
            FillTestVideoItem(vi2);
            vi2.ID = "vi2";

            ICred cred = _crf.CreateCred();
            FillTestCred(cred);

            IChannel ch = _cf.CreateChannel();
            FillTestChannel(ch, vi, vi2, cred);

            IPlaylist pl = _pf.CreatePlaylist();
            FillTestPl(pl, ch);

            // DeleteCredAsync
            Task t = _db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);

            // InsertCredAsync
            t = _db.InsertCredAsync(cred);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // InsertChannelItemsAsync
            t = _db.InsertChannelItemsAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            // DeletePlaylistAsync
            t = _db.DeletePlaylistAsync(pl.ID);
            Assert.IsTrue(!t.IsFaulted);

            // InsertPlaylistAsync
            t = _db.InsertPlaylistAsync(pl);
            Assert.IsTrue(!t.IsFaulted);

            // GetPlaylistAsync
            t = _db.GetPlaylistAsync(pl.ID);
            Assert.IsTrue(!t.IsFaulted);

            // GetChannelPlaylistAsync
            t = _db.GetChannelPlaylistAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // UpdatePlaylistAsync
            t = _db.UpdatePlaylistAsync(pl.ID, vi.ID, ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // GetPlaylistItemsAsync
            t = _db.GetPlaylistItemsAsync(pl.ID, ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeletePlaylistAsync
            t = _db.DeletePlaylistAsync(pl.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteCredAsync
            t = _db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public void TestCrudSettings()
        {
            ISqLiteDatabase db = _factory.CreateSqLiteDatabase();

            ISetting setting = _sf.CreateSetting();
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
            ITag tag = _tf.CreateTag();
            FillTestTag(tag);

            // DeleteTagAsync
            Task t = _db.DeleteTagAsync(tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            // InsertTagAsync
            t = _db.InsertTagAsync(tag);
            Assert.IsTrue(!t.IsFaulted);

            IVideoItem vi = _vf.CreateVideoItem();
            FillTestVideoItem(vi);

            IVideoItem vi2 = _vf.CreateVideoItem();
            FillTestVideoItem(vi2);
            vi2.ID = "vi2";

            ICred cred = _crf.CreateCred();
            FillTestCred(cred);

            IChannel ch = _cf.CreateChannel();
            FillTestChannel(ch, vi, vi2, cred);

            // DeleteCredAsync
            t = _db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);

            // InsertCredAsync
            t = _db.InsertCredAsync(cred);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // InsertChannelAsync
            t = _db.InsertChannelAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            // InsertChannelTagsAsync
            t = _db.InsertChannelTagsAsync(ch.ID, tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            // InsertChannelTagsAsync
            t = _db.InsertChannelTagsAsync(ch.ID, tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            // GetChannelTagsAsync
            t = _db.GetChannelTagsAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // GetChannelsByTagAsync
            t = _db.GetChannelsByTagAsync(tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelTagsAsync
            t = _db.DeleteChannelTagsAsync(ch.ID, tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteChannelAsync
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteTagAsync
            t = _db.DeleteTagAsync(tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            // DeleteCredAsync
            t = _db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);
        }

        #endregion
    }
}
