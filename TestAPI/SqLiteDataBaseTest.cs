using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Interfaces.API;
using Interfaces.Factories;
using Interfaces.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace TestAPI
{
    [TestClass]
    public class SqLiteDataBaseTest
    {
        private const string TestPhoto =
            "https://yt3.ggpht.com/-qMfsaQGvdSQ/AAAAAAAAAAI/AAAAAAAAAAA/NLFnUAWeiaM/s88-c-k-no/photo.jpg";

        private readonly ICommonFactory _factory;

        private readonly ISqLiteDatabase _db;

        private readonly IVideoItemFactory _vf;

        private readonly IChannelFactory _cf;

        private readonly ICredFactory _crf;

        private readonly IPlaylistFactory _pf;

        private readonly ITagFactory _tf;

        private readonly ISettingFactory _sf;

        public SqLiteDataBaseTest()
        {
            _factory = IoC.Container.Kernel.Get<ICommonFactory>();
            _vf = _factory.CreateVideoItemFactory();
            _db = _factory.CreateSqLiteDatabase();
            _cf = _factory.CreateChannelFactory();
            _crf = _factory.CreateCredFactory();
            _pf = _factory.CreatePlaylistFactory();
            _tf = _factory.CreateTagFactory();
            _sf = _factory.CreateSettingFactory();
        }

        [TestMethod]
        public void TestCrudItems()
        {
            //var db = new Mock<SqLiteDatabase>();

            var vi = _vf.CreateVideoItem();
            //var vi = new Mock<IVideoItem>();
            //vi.SetupAllProperties();
            FillTestVideoItem(vi);

            var vi2 = _vf.CreateVideoItem();
            FillTestVideoItem(vi2);
            vi2.ID = "vi2";

            //var cred = new Mock<ICred>();
            var cred = _crf.CreateCred();
            //cred.SetupAllProperties();
            FillTestCred(cred);

            var ch = _cf.CreateChannel();
            //var ch = new Mock<IChannel>();
            //ch.SetupAllProperties();
            FillTestChannel(ch, vi, vi2, cred);

            Task t;
            
            //DeleteCredAsync
            //t = db.Object.DeleteCredAsync(cred.Object.Site);
            t = _db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);

            //InsertCredAsync
            //t = db.Object.InsertCredAsync(cred.Object);
            t = _db.InsertCredAsync(cred);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            //t = db.Object.DeleteChannelAsync(ch.Object.ID);
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //InsertChannelAsync
            //t = db.Object.InsertChannelAsync(ch.Object);
            t = _db.InsertChannelAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            //RenameChannelAsync
            //t = db.Object.RenameChannelAsync(ch.Object.ID, "newname");
            t = _db.RenameChannelAsync(ch.ID, "newname");
            Assert.IsTrue(!t.IsFaulted);

            //GetChannelAsync
            //t = db.Object.GetChannelAsync(ch.Object.ID);
            t = _db.GetChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //GetChannelsListAsync
            //t = db.Object.GetChannelsListAsync();
            t = _db.GetChannelsListAsync();
            Assert.IsTrue(!t.IsFaulted);

            //GetChannelItemsAsync
            //t = db.Object.GetChannelItemsAsync(ch.Object.ID);
            t = _db.GetChannelItemsAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //GetChannelItemsCountDbAsync
            t = _db.GetChannelItemsCountDbAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            //t = db.Object.DeleteChannelAsync(ch.Object.ID);
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //InsertChannelItemsAsync
            //t = db.Object.InsertChannelItemsAsync(ch.Object);
            t = _db.InsertChannelItemsAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            //t = db.Object.DeleteChannelAsync(ch.Object.ID);
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //ITEMS

            //InsertChannelAsync
            //t = db.Object.InsertChannelAsync(ch.Object);
            t = _db.InsertChannelAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            //InsertItemAsync
            //t = db.Object.InsertItemAsync(vi.Object);
            t = _db.InsertItemAsync(vi);
            Assert.IsTrue(!t.IsFaulted);

            //GetVideoItemAsync
            //t = db.Object.GetVideoItemAsync(vi.Object.ID);
            t = _db.GetVideoItemAsync(vi.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteItemAsync
            //t = db.Object.DeleteItemAsync(vi.Object.ID);
            t = _db.DeleteItemAsync(vi.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            //t = db.Object.DeleteChannelAsync(ch.Object.ID);
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteCredAsync
            //t = db.Object.DeleteCredAsync(cred.Object.Site);
            t = _db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public void TestCrudPlaylists()
        {
            //var db = new Mock<SqLiteDatabase>();

            var vi = _vf.CreateVideoItem();
            FillTestVideoItem(vi);

            var vi2 = _vf.CreateVideoItem();
            FillTestVideoItem(vi2);
            vi2.ID = "vi2";

            var cred = _crf.CreateCred();
            FillTestCred(cred);

            var ch = _cf.CreateChannel();
            FillTestChannel(ch, vi, vi2, cred);

            var pl = _pf.CreatePlaylist();
            FillTestPl(pl, ch);

            Task t;

            //DeleteCredAsync
            t = _db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);

            //InsertCredAsync
            t = _db.InsertCredAsync(cred);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //InsertChannelItemsAsync
            t = _db.InsertChannelItemsAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            //DeletePlaylistAsync
            t = _db.DeletePlaylistAsync(pl.ID);
            Assert.IsTrue(!t.IsFaulted);

            //InsertPlaylistAsync
            t = _db.InsertPlaylistAsync(pl);
            Assert.IsTrue(!t.IsFaulted);

            //GetPlaylistAsync
            t = _db.GetPlaylistAsync(pl.ID);
            Assert.IsTrue(!t.IsFaulted);

            //GetChannelPlaylistAsync
            t = _db.GetChannelPlaylistAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //UpdatePlaylistAsync
            t = _db.UpdatePlaylistAsync(pl.ID, vi.ID, ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //GetPlaylistItemsAsync
            t = _db.GetPlaylistItemsAsync(pl.ID, ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeletePlaylistAsync
            t = _db.DeletePlaylistAsync(pl.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteCredAsync
            t = _db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public void TestCrudTags()
        {
            //var db = new Mock<SqLiteDatabase>();

            var tag = _tf.CreateTag();
            FillTestTag(tag);

            Task t;

            //DeleteTagAsync
            t = _db.DeleteTagAsync(tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            //InsertTagAsync
            t = _db.InsertTagAsync(tag);
            Assert.IsTrue(!t.IsFaulted);

            var vi = _vf.CreateVideoItem();
            FillTestVideoItem(vi);

            var vi2 = _vf.CreateVideoItem();
            FillTestVideoItem(vi2);
            vi2.ID = "vi2";

            var cred = _crf.CreateCred();
            FillTestCred(cred);

            var ch = _cf.CreateChannel();
            FillTestChannel(ch, vi, vi2, cred);

            //DeleteCredAsync
            t = _db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);

            //InsertCredAsync
            t = _db.InsertCredAsync(cred);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //InsertChannelAsync
            t = _db.InsertChannelAsync(ch);
            Assert.IsTrue(!t.IsFaulted);

            //InsertChannelTagsAsync
            t = _db.InsertChannelTagsAsync(ch.ID, tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            //InsertChannelTagsAsync
            t = _db.InsertChannelTagsAsync(ch.ID, tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            //GetChannelTagsAsync
            t = _db.GetChannelTagsAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //GetChannelsByTagAsync
            t = _db.GetChannelsByTagAsync(tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelTagsAsync
            t = _db.DeleteChannelTagsAsync(ch.ID, tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            t = _db.DeleteChannelAsync(ch.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteTagAsync
            t = _db.DeleteTagAsync(tag.Title);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteCredAsync
            t = _db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);

        }

        [TestMethod]
        public void TestCrudCredentials()
        {
            //var db = new Mock<SqLiteDatabase>();

            var db = _factory.CreateSqLiteDatabase();

            var cred = _crf.CreateCred();
            FillTestCred(cred);

            Task t;

            //DeleteCredAsync
            t = db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);

            //InsertCredAsync
            t = db.InsertCredAsync(cred);
            Assert.IsTrue(!t.IsFaulted);

            //GetCredAsync
            t = db.GetCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);

            //UpdateLoginAsync
            t = db.UpdateLoginAsync(cred.Site, "newlogin");
            Assert.IsTrue(!t.IsFaulted);

            //UpdatePasswordAsync
            t = db.UpdatePasswordAsync(cred.Site, "newpassword");
            Assert.IsTrue(!t.IsFaulted);

            //UpdateAutorizationAsync
            t = db.UpdateAutorizationAsync(cred.Site, 1);
            Assert.IsTrue(!t.IsFaulted);

            //GetCredListAsync
            t = db.GetCredListAsync();
            Assert.IsTrue(!t.IsFaulted);

            //DeleteCredAsync
            t = db.DeleteCredAsync(cred.Site);
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public void TestCrudSettings()
        {
            //var db = new Mock<SqLiteDatabase>();

            var db = _factory.CreateSqLiteDatabase();

            var setting = _sf.CreateSetting();
            FillTestSetting(setting);

            Task t;

            //DeleteSettingAsync
            t = db.DeleteSettingAsync(setting.Key);
            Assert.IsTrue(!t.IsFaulted);

            //InsertSettingAsync
            t = db.InsertSettingAsync(setting);
            Assert.IsTrue(!t.IsFaulted);

            //UpdateSettingAsync
            t = db.UpdateSettingAsync(setting.Key, "newvalue");
            Assert.IsTrue(!t.IsFaulted);

            //GetSettingAsync
            t = db.GetSettingAsync(setting.Key);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteSettingAsync
            t = db.DeleteSettingAsync(setting.Key);
            Assert.IsTrue(!t.IsFaulted);
        }

        private static void FillTestCred(ICred cred)
        {
            cred.Site = "testsite.com";
            cred.Login = "testlogin";
            cred.Pass = "testpass";
            cred.Cookie = "cookie";
            cred.Passkey = "testkey";
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

        private static void FillTestChannel(IChannel ch, IVideoItem v1, IVideoItem v2, ICred cred)
        {
            ch.ChannelItems = new ObservableCollection<IVideoItem>();
            ch.ID = "testch";
            ch.Title = "тестовая канал, для отладки слоя бд";
            ch.SubTitle = "использутеся для отдладки :)";
            //ch.LastUpdated = DateTime.MinValue;
            ch.Thumbnail = GetStreamFromUrl(TestPhoto);
            ch.Site = cred.Site;
            ch.ChannelItems.Add(v1);
            ch.ChannelItems.Add(v2);
        }

        private static void FillTestTag(ITag tag)
        {
            tag.Title = "testag";
        }

        private static void FillTestSetting(ISetting setting)
        {
            setting.Key = "testsetting";
            setting.Value = "testvalue";
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
    }
}
