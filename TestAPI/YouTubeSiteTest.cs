// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAPI.POCO;
using DataAPI.Videos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models.Factories;

namespace TestAPI
{
    [TestClass]
    public class YouTubeSiteTest
    {
        #region Static and Readonly Fields

        private readonly YouTubeSite you;

        #endregion

        #region Constructors

        public YouTubeSiteTest()
        {
            you = CommonFactory.CreateYouTubeSite();
        }

        #endregion

        #region Methods

        [TestMethod]
        public async Task GetChannelIdByUserNameNetAsync()
        {
            string res = await YouTubeSite.GetChannelIdByUserNameNetAsync("mcmbmirussian");

            Assert.AreEqual(res, "UCH0miwnqCojki-ado_lLI5A");

            res = await YouTubeSite.GetChannelIdByUserNameNetAsync("CarCrashCompilation7");

            Assert.AreEqual("UCeXeMXzjt21uv5tonZHtOrA", res);
        }

        [TestMethod]
        public async Task GetChannelItemsAsync()
        {
            // UCGtFbbGApG0s_8mMuJd-zKg = berestian
            // UCQoZVSaWvaJN046F-8SmyPg
            // UCq9B1wrqZKwucNkjHnUW39A
            const int count = 2;
            IEnumerable<VideoItemPOCO> lst = await you.GetChannelItemsAsync("UCGtFbbGApG0s_8mMuJd-zKg", count);
            Assert.AreEqual(count, lst.Count());
        }

        [TestMethod]
        public async Task GetChannelItemsCountNetAsync()
        {
            int res = await YouTubeSite.GetChannelItemsCountNetAsync("UCE27j85FZ8-aZOn6D8vWMWg");

            Assert.AreEqual(res, 6);
        }

        [TestMethod]
        public async Task GetChannelItemsIdsListNetAsync()
        {
            IEnumerable<string> res = await you.GetChannelItemsIdsListNetAsync("UCE27j85FZ8-aZOn6D8vWMWg", 5);

            Assert.AreEqual(res.Count(), 5);
        }

        [TestMethod]
        public async Task GetChannelNetAsync()
        {
            ChannelPOCO res = await YouTubeSite.GetChannelNetAsync("UCE27j85FZ8-aZOn6D8vWMWg");

            Assert.AreEqual(res.Title, "Vlad RT");
        }

        [TestMethod]
        public async Task GetChannelPlaylistNetAsync()
        {
            IEnumerable<PlaylistPOCO> res = await YouTubeSite.GetChannelPlaylistsNetAsync("UCq9B1wrqZKwucNkjHnUW39A");

            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetChannelRelatedPlaylistsNetAsync()
        {
            IEnumerable<PlaylistPOCO> lst = await YouTubeSite.GetChannelRelatedPlaylistsNetAsync("UC0lT9K8Wfuc1KPqm6YjRf1A");
            Assert.AreEqual(lst.Count(), 3);
        }

        [TestMethod]
        public async Task GetFullChannel()
        {
            ChannelPOCO channel = await you.GetChannelFullNetAsync("UCeXeMXzjt21uv5tonZHtOrA");
            int count = await YouTubeSite.GetChannelItemsCountNetAsync("UCeXeMXzjt21uv5tonZHtOrA");
            Assert.IsTrue(channel.Items.Count == count);
            Assert.IsTrue(channel.Items.Any());
            Assert.IsTrue(channel.Playlists.Any());
        }

        [TestMethod]
        public async Task GetListVideoByIdsAsync()
        {
            var lst = new List<string> { "-wA6Qj4oF2E" };

            IEnumerable<VideoItemPOCO> res = await you.GetVideosListByIdsAsync(lst);

            Assert.AreEqual(res.Count(), 1);
        }

        [TestMethod]
        public async Task GetPlaylistItemsCountNetAsync()
        {
            int res = await YouTubeSite.GetPlaylistItemsCountNetAsync("UU0lT9K8Wfuc1KPqm6YjRf1A");

            int res2 = await YouTubeSite.GetChannelItemsCountNetAsync("UC0lT9K8Wfuc1KPqm6YjRf1A");

            Assert.IsTrue(res == res2);
        }

        [TestMethod]
        public async Task GetPlaylistItemsIdsListNetAsync()
        {
            IEnumerable<string> res = await YouTubeSite.GetPlaylistItemsIdsListNetAsync("PLt2cGgt6G8WrItA7KTI5m6EFniMfphWJC", 0);

            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetPlaylistItemsNetAsync()
        {
            IEnumerable<VideoItemPOCO> lst = await you.GetPlaylistItemsNetAsync("UU0lT9K8Wfuc1KPqm6YjRf1A");

            Assert.IsTrue(lst.Any());
        }

        [TestMethod]
        public async Task GetPlaylistNetAsync()
        {
            PlaylistPOCO res = await YouTubeSite.GetPlaylistNetAsync("PLt2cGgt6G8WrItA7KTI5m6EFniMfphWJC");

            Assert.AreEqual(res.Title, "Creating Windows Services");

            Assert.AreEqual(res.SubTitle, "How to create, install and deploy windows services.");
        }

        [TestMethod]
        public async Task GetPopularItemsAsync()
        {
            // var testindex = new[] { 1, 2, 25, 26, 27, 49, 50, 51 };
            var testindex = new[] { 2 };
            foreach (int i in testindex)
            {
                IEnumerable<VideoItemPOCO> lst = await you.GetPopularItemsAsync("ru", i);
                Assert.AreEqual(lst.Count(), i);
            }
        }

        [TestMethod]
        public async Task GetRelatedChannelsByIdAsync()
        {
            IEnumerable<ChannelPOCO> res = await YouTubeSite.GetRelatedChannelsByIdAsync("UCsNGRSN63gFoo5z6Oqv1A6A");
            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetVideoItemNetAsync()
        {
            VideoItemPOCO res = await you.GetVideoItemNetAsync("lHgIpxQac3w"); // 

            Assert.AreEqual(res.Title, "Metallica — Unforgiven (FDM edition)");
        }

        [TestMethod]
        public async Task GetVideoSubtitlesByIdAsync()
        {
            IEnumerable<SubtitlePOCO> res = await YouTubeSite.GetVideoSubtitlesByIdAsync("WaEcvDnbaIc");
            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task SearchItemsAsync()
        {
            var testindex = new[] { 5 };
            foreach (int i in testindex)
            {
                IEnumerable<VideoItemPOCO> lst = await you.SearchItemsAsync("russia", "RU", i);
                Assert.AreEqual(lst.Count(), i);
            }
        }

        #endregion
    }
}
