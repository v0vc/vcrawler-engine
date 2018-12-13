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

namespace TestAPI
{
    [TestClass]
    public class YouTubeSiteTest
    {
        #region Methods

        [TestMethod]
        public async Task GetChannelIdByUserNameNetAsync()
        {
            string res = await YouTubeSite.GetChannelIdByUserNameNetAsync("mcmbmirussian").ConfigureAwait(false);

            Assert.AreEqual(res, "UCH0miwnqCojki-ado_lLI5A");

            res = await YouTubeSite.GetChannelIdByUserNameNetAsync("CarCrashCompilation7").ConfigureAwait(false);

            Assert.AreEqual("UCeXeMXzjt21uv5tonZHtOrA", res);
        }

        [TestMethod]
        public async Task GetChannelItemsAsync()
        {
            // UCGtFbbGApG0s_8mMuJd-zKg = berestian
            // UCQoZVSaWvaJN046F-8SmyPg
            // UCq9B1wrqZKwucNkjHnUW39A
            const int count = 2;
            IEnumerable<VideoItemPOCO> lst =
                await YouTubeSite.GetChannelItemsAsync("UCGtFbbGApG0s_8mMuJd-zKg", count).ConfigureAwait(false);
            Assert.AreEqual(count, lst.Count());
        }

        [TestMethod]
        public async Task GetChannelItemsCountNetAsync()
        {
            int res = await YouTubeSite.GetChannelItemsCountNetAsync("UCE27j85FZ8-aZOn6D8vWMWg").ConfigureAwait(false);

            Assert.AreEqual(res, 6);
        }

        [TestMethod]
        public async Task GetChannelItemsIdsListNetAsync()
        {
            List<string> res = await YouTubeSite.GetChannelItemsIdsListNetAsync("UCE27j85FZ8-aZOn6D8vWMWg", 5).ConfigureAwait(false);

            Assert.AreEqual(res.Count(), 5);
        }

        [TestMethod]
        public async Task GetChannelNetAsync()
        {
            ChannelPOCO res = await YouTubeSite.GetChannelNetAsync("UCE27j85FZ8-aZOn6D8vWMWg").ConfigureAwait(false);

            Assert.AreEqual(res.Title, "Vlad RT");
        }

        [TestMethod]
        public async Task GetChannelPlaylistNetAsync()
        {
            List<PlaylistPOCO> res = await YouTubeSite.GetChannelPlaylistsNetAsync("UCq9B1wrqZKwucNkjHnUW39A").ConfigureAwait(false);

            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetChannelRelatedPlaylistsNetAsync()
        {
            List<PlaylistPOCO> lst =
                await YouTubeSite.GetChannelRelatedPlaylistsNetAsync("UC0lT9K8Wfuc1KPqm6YjRf1A").ConfigureAwait(false);
            Assert.AreEqual(lst.Count(), 3);
        }

        [TestMethod]
        public async Task GetFullChannel()
        {
            ChannelPOCO channel = await YouTubeSite.GetChannelFullNetAsync("UCeXeMXzjt21uv5tonZHtOrA").ConfigureAwait(false);
            int count = await YouTubeSite.GetChannelItemsCountNetAsync("UCeXeMXzjt21uv5tonZHtOrA").ConfigureAwait(false);
            Assert.IsTrue(channel.Items.Count == count);
            Assert.IsTrue(channel.Items.Any());
            Assert.IsTrue(channel.Playlists.Any());
        }

        [TestMethod]
        public async Task GetListVideoByIdsAsync()
        {
            var lst = new List<string> { "-wA6Qj4oF2E" };

            List<VideoItemPOCO> res = await YouTubeSite.GetVideosListByIdsAsync(lst).ConfigureAwait(false);

            Assert.AreEqual(res.Count(), 1);
        }

        [TestMethod]
        public async Task GetPlaylistItemsCountNetAsync()
        {
            int res = await YouTubeSite.GetPlaylistItemsCountNetAsync("UU0lT9K8Wfuc1KPqm6YjRf1A").ConfigureAwait(false);

            int res2 = await YouTubeSite.GetChannelItemsCountNetAsync("UC0lT9K8Wfuc1KPqm6YjRf1A").ConfigureAwait(false);

            Assert.IsTrue(res == res2);
        }

        [TestMethod]
        public async Task GetPlaylistItemsIdsListNetAsync()
        {
            IEnumerable<string> res =
                await YouTubeSite.GetPlaylistItemsIdsListNetAsync("PLt2cGgt6G8WrItA7KTI5m6EFniMfphWJC", 0).ConfigureAwait(false);

            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetPlaylistItemsNetAsync()
        {
            List<VideoItemPOCO> lst = await YouTubeSite.GetPlaylistItemsNetAsync("UU0lT9K8Wfuc1KPqm6YjRf1A").ConfigureAwait(false);

            Assert.IsTrue(lst.Any());
        }

        [TestMethod]
        public async Task GetPlaylistNetAsync()
        {
            PlaylistPOCO res = await YouTubeSite.GetPlaylistNetAsync("PLt2cGgt6G8WrItA7KTI5m6EFniMfphWJC").ConfigureAwait(false);

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
                List<VideoItemPOCO> lst = await YouTubeSite.GetPopularItemsAsync("ru", i).ConfigureAwait(false);
                Assert.AreEqual(lst.Count(), i);
            }
        }

        [TestMethod]
        public async Task GetRelatedChannelsByIdAsync()
        {
            List<ChannelPOCO> res = await YouTubeSite.GetRelatedChannelsByIdAsync("UCsNGRSN63gFoo5z6Oqv1A6A").ConfigureAwait(false);
            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetVideoCommentsNetAsync()
        {
            List<string> res = await YouTubeSite.GetVideoCommentsNetAsync("-wA6Qj4oF2E", 0).ConfigureAwait(false);
            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetVideoItemNetAsync()
        {
            VideoItemPOCO res = await YouTubeSite.GetVideoItemNetAsync("lHgIpxQac3w").ConfigureAwait(false); // 

            Assert.AreEqual(res.Title, "Metallica — Unforgiven (FDM edition)");
        }

        [TestMethod]
        public async Task GetVideoViewCountNetAsync()
        {
            var res = await YouTubeSite.GetVideoViewCountNetAsync("lHgIpxQac3w").ConfigureAwait(false);
            Assert.IsTrue(res.ViewCount > 0);
        }

        [TestMethod]
        public async Task GetVideoRateCountNetAsync()
        {
            var res = await YouTubeSite.GetVideoRateCountNetAsync(new List<string> { "lHgIpxQac3w", "-wA6Qj4oF2E" }).ConfigureAwait(false);
            Assert.IsTrue(res.Count == 2);
            var res1 = await YouTubeSite.GetVideoViewCountNetAsync("lHgIpxQac3w").ConfigureAwait(false);
            Assert.IsTrue(res[0].ViewCount == res1.ViewCount);
        }

        [TestMethod]
        public async Task GetVideoSubtitlesByIdAsync()
        {
            List<SubtitlePOCO> res = await YouTubeSite.GetVideoSubtitlesByIdAsync("WaEcvDnbaIc").ConfigureAwait(false);
            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task SearchItemsAsync()
        {
            var testindex = new[] { 5 };
            foreach (int i in testindex)
            {
                List<VideoItemPOCO> lst = await YouTubeSite.SearchItemsAsync("russia", "RU", i).ConfigureAwait(false);
                Assert.AreEqual(lst.Count(), i);
            }
        }

        #endregion
    }
}
