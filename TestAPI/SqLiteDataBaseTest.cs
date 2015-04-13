using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DataBaseAPI;
using Interfaces.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TestAPI
{
    [TestClass]
    public class SqLiteDataBaseTest
    {
        [TestMethod]
        public void TestCrudItems()
        {
            var db = new Mock<SqLiteDatabase>();

            var vi = new Mock<IVideoItem>();
            vi.SetupAllProperties();
            FillTestVideoItem(vi.Object);

            var vi2 = new Mock<IVideoItem>();
            vi2.SetupAllProperties();
            FillTestVideoItem(vi2.Object);
            vi2.Object.ID = "vi2";

            var cred = new Mock<ICred>();
            cred.SetupAllProperties();
            FillTestCred(cred.Object);

            var ch = new Mock<IChannel>();
            ch.SetupAllProperties();
            FillTestChannel(ch.Object, vi.Object, vi2.Object, cred.Object);

            Task t;

            //DeleteCredAsync
            t = db.Object.DeleteCredAsync(cred.Object.Site);
            Assert.IsTrue(!t.IsFaulted);

            //InsertCredAsync
            t = db.Object.InsertCredAsync(cred.Object);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            t = db.Object.DeleteChannelAsync(ch.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //InsertChannelAsync
            t = db.Object.InsertChannelAsync(ch.Object);
            Assert.IsTrue(!t.IsFaulted);

            //RenameChannelAsync
            t = db.Object.RenameChannelAsync(ch.Object.ID, "newname");
            Assert.IsTrue(!t.IsFaulted);

            //GetChannelAsync
            t = db.Object.GetChannelAsync(ch.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //GetChannelsListAsync
            t = db.Object.GetChannelsListAsync();
            Assert.IsTrue(!t.IsFaulted);

            //GetChannelItemsAsync
            t = db.Object.GetChannelItemsAsync(ch.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            t = db.Object.DeleteChannelAsync(ch.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //InsertChannelItemsAsync
            t = db.Object.InsertChannelItemsAsync(ch.Object);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            t = db.Object.DeleteChannelAsync(ch.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //ITEMS

            //InsertChannelAsync
            t = db.Object.InsertChannelAsync(ch.Object);
            Assert.IsTrue(!t.IsFaulted);

            //InsertItemAsync
            t = db.Object.InsertItemAsync(vi.Object);
            Assert.IsTrue(!t.IsFaulted);

            //GetVideoItemAsync
            t = db.Object.GetVideoItemAsync(vi.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteItemAsync
            t = db.Object.DeleteItemAsync(vi.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            t = db.Object.DeleteChannelAsync(ch.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteCredAsync
            t = db.Object.DeleteCredAsync(cred.Object.Site);
            Assert.IsTrue(!t.IsFaulted);

        }

        [TestMethod]
        public void TestCrudPlaylists()
        {
            var db = new Mock<SqLiteDatabase>();

            var vi = new Mock<IVideoItem>();
            vi.SetupAllProperties();
            FillTestVideoItem(vi.Object);

            var vi2 = new Mock<IVideoItem>();
            vi2.SetupAllProperties();
            FillTestVideoItem(vi2.Object);
            vi2.Object.ID = "vi2";

            var cred = new Mock<ICred>();
            cred.SetupAllProperties();
            FillTestCred(cred.Object);

            var ch = new Mock<IChannel>();
            ch.SetupAllProperties();
            FillTestChannel(ch.Object, vi.Object, vi2.Object, cred.Object);

            var pl = new Mock<IPlaylist>();
            pl.SetupAllProperties();
            FillTestPl(pl.Object, ch.Object);

            Task t;

            //DeleteCredAsync
            t = db.Object.DeleteCredAsync(cred.Object.Site);
            Assert.IsTrue(!t.IsFaulted);

            //InsertCredAsync
            t = db.Object.InsertCredAsync(cred.Object);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            t = db.Object.DeleteChannelAsync(ch.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //InsertChannelItemsAsync
            t = db.Object.InsertChannelItemsAsync(ch.Object);
            Assert.IsTrue(!t.IsFaulted);

            //DeletePlaylistAsync
            t = db.Object.DeletePlaylistAsync(pl.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //InsertPlaylistAsync
            t = db.Object.InsertPlaylistAsync(pl.Object);
            Assert.IsTrue(!t.IsFaulted);

            //GetPlaylistAsync
            t = db.Object.GetPlaylistAsync(pl.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //GetChannelPlaylistAsync
            t = db.Object.GetChannelPlaylistAsync(ch.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //UpdatePlaylistAsync
            t = db.Object.UpdatePlaylistAsync(pl.Object.ID, vi.Object.ID, ch.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //GetPlaylistItemsAsync
            t = db.Object.GetPlaylistItemsAsync(pl.Object.ID, ch.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeletePlaylistAsync
            t = db.Object.DeletePlaylistAsync(pl.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            t = db.Object.DeleteChannelAsync(ch.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteCredAsync
            t = db.Object.DeleteCredAsync(cred.Object.Site);
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public void TestCrudTags()
        {
            var db = new Mock<SqLiteDatabase>();

            var tag = new Mock<ITag>();
            tag.SetupAllProperties();
            FillTestTag(tag.Object);

            Task t;

            //DeleteTagAsync
            t = db.Object.DeleteTagAsync(tag.Object.Title);
            Assert.IsTrue(!t.IsFaulted);

            //InsertTagAsync
            t = db.Object.InsertTagAsync(tag.Object);
            Assert.IsTrue(!t.IsFaulted);

            var vi = new Mock<IVideoItem>();
            vi.SetupAllProperties();
            FillTestVideoItem(vi.Object);

            var vi2 = new Mock<IVideoItem>();
            vi2.SetupAllProperties();
            FillTestVideoItem(vi2.Object);
            vi2.Object.ID = "vi2";

            var cred = new Mock<ICred>();
            cred.SetupAllProperties();
            FillTestCred(cred.Object);

            var ch = new Mock<IChannel>();
            ch.SetupAllProperties();
            FillTestChannel(ch.Object, vi.Object, vi2.Object, cred.Object);

            //DeleteCredAsync
            t = db.Object.DeleteCredAsync(cred.Object.Site);
            Assert.IsTrue(!t.IsFaulted);

            //InsertCredAsync
            t = db.Object.InsertCredAsync(cred.Object);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            t = db.Object.DeleteChannelAsync(ch.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //InsertChannelAsync
            t = db.Object.InsertChannelAsync(ch.Object);
            Assert.IsTrue(!t.IsFaulted);

            //InsertChannelTagsAsync
            t = db.Object.InsertChannelTagsAsync(ch.Object.ID, tag.Object.Title);
            Assert.IsTrue(!t.IsFaulted);

            //InsertChannelTagsAsync
            t = db.Object.InsertChannelTagsAsync(ch.Object.ID, tag.Object.Title);
            Assert.IsTrue(!t.IsFaulted);

            //GetChannelTagsAsync
            t = db.Object.GetChannelTagsAsync(ch.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //GetChannelsByTagAsync
            t = db.Object.GetChannelsByTagAsync(tag.Object.Title);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelTagsAsync
            t = db.Object.DeleteChannelTagsAsync(ch.Object.ID, tag.Object.Title);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteChannelAsync
            t = db.Object.DeleteChannelAsync(ch.Object.ID);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteTagAsync
            t = db.Object.DeleteTagAsync(tag.Object.Title);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteCredAsync
            t = db.Object.DeleteCredAsync(cred.Object.Site);
            Assert.IsTrue(!t.IsFaulted);

        }

        [TestMethod]
        public void TestCrudCredentials()
        {
            var db = new Mock<SqLiteDatabase>();

            var cred = new Mock<ICred>();
            cred.SetupAllProperties();
            FillTestCred(cred.Object);

            Task t;

            //DeleteCredAsync
            t = db.Object.DeleteCredAsync(cred.Object.Site);
            Assert.IsTrue(!t.IsFaulted);

            //InsertCredAsync
            t = db.Object.InsertCredAsync(cred.Object);
            Assert.IsTrue(!t.IsFaulted);

            //GetCredAsync
            t = db.Object.GetCredAsync(cred.Object.Site);
            Assert.IsTrue(!t.IsFaulted);

            //UpdateLoginAsync
            t = db.Object.UpdateLoginAsync(cred.Object.Site, "newlogin");
            Assert.IsTrue(!t.IsFaulted);

            //UpdatePasswordAsync
            t = db.Object.UpdatePasswordAsync(cred.Object.Site, "newpassword");
            Assert.IsTrue(!t.IsFaulted);

            //UpdateAutorizationAsync
            t = db.Object.UpdateAutorizationAsync(cred.Object.Site, 1);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteCredAsync
            t = db.Object.DeleteCredAsync(cred.Object.Site);
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public void TestCrudSettings()
        {
            var db = new Mock<SqLiteDatabase>();

            var setting = new Mock<ISetting>();
            setting.SetupAllProperties();
            FillTestSetting(setting.Object);

            Task t;

            //DeleteSettingAsync
            t = db.Object.DeleteSettingAsync(setting.Object.Key);
            Assert.IsTrue(!t.IsFaulted);

            //InsertSettingAsync
            t = db.Object.InsertSettingAsync(setting.Object);
            Assert.IsTrue(!t.IsFaulted);

            //UpdateSettingAsync
            t = db.Object.UpdateSettingAsync(setting.Object.Key, "newvalue");
            Assert.IsTrue(!t.IsFaulted);

            //GetSettingAsync
            t = db.Object.GetSettingAsync(setting.Object.Key);
            Assert.IsTrue(!t.IsFaulted);

            //DeleteSettingAsync
            t = db.Object.DeleteSettingAsync(setting.Object.Key);
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
            pl.Link = "https://link.com";
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
            ch.ChannelItems = new List<IVideoItem>();
            ch.ID = "testch";
            ch.Title = "тестовая канал, для отладки слоя бд";
            ch.SubTitle = "использутеся для отдладки :)";
            ch.LastUpdated = DateTime.MinValue;
            ch.Thumbnail = GetStreamFromUrl("https://yt3.ggpht.com/-qMfsaQGvdSQ/AAAAAAAAAAI/AAAAAAAAAAA/NLFnUAWeiaM/s88-c-k-no/photo.jpg");
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
