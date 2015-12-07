// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using DataAPI.Videos;
using Interfaces.Models;
using Interfaces.POCO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Models.Factories;

namespace TestAPI
{
    [TestClass]
    public class YouTubeSiteTest
    {
        #region Static and Readonly Fields

        private readonly CommonFactory fabric;

        #endregion

        #region Fields

        private ICred cred;

        #endregion

        #region Constructors

        public YouTubeSiteTest()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                fabric = scope.Resolve<CommonFactory>();
            }
            FillCred();
        }

        #endregion

        #region Methods

        [TestMethod]
        public async Task GetFullChannel()
        {
            var you = GetYouFabric();
            var channel = await you.GetChannelFullNetAsync("UCeXeMXzjt21uv5tonZHtOrA");
            var count = await you.GetChannelItemsCountNetAsync("UCeXeMXzjt21uv5tonZHtOrA");
            Assert.IsTrue(channel.Items.Count == count);
            Assert.IsTrue(channel.Items.Any());
            Assert.IsTrue(channel.Playlists.Any());
        }

        [TestMethod]
        public async Task GetChannelIdByUserNameNetAsync()
        {
            var you = GetYouFabric();

            string res = await you.GetChannelIdByUserNameNetAsync("mcmbmirussian");

            Assert.AreEqual(res, "UCH0miwnqCojki-ado_lLI5A");

            string res1 = await you.GetChannelIdByUserNameNetAsync("CarCrashCompilation7");

            Assert.IsFalse(string.IsNullOrEmpty(res1));
        }

        [TestMethod]
        public async Task GetChannelItemsAsync()
        {
            var you = GetYouFabric();

            // UCQoZVSaWvaJN046F-8SmyPg
            // UCq9B1wrqZKwucNkjHnUW39A
            IEnumerable<IVideoItemPOCO> lst = await you.GetChannelItemsAsync("UCQoZVSaWvaJN046F-8SmyPg", 5);

            Assert.AreEqual(lst.Count(), 5);
        }

        [TestMethod]
        public async Task GetChannelItemsCountNetAsync()
        {
            var you = GetYouFabric();

            int res = await you.GetChannelItemsCountNetAsync("UCE27j85FZ8-aZOn6D8vWMWg");

            Assert.AreEqual(res, 8);
        }

        [TestMethod]
        public async Task GetChannelItemsIdsListNetAsync()
        {
            var you = GetYouFabric();

            IEnumerable<string> res = await you.GetChannelItemsIdsListNetAsync("UCE27j85FZ8-aZOn6D8vWMWg", 5);

            Assert.AreEqual(res.Count(), 5);
        }

        [TestMethod]
        public async Task GetChannelNetAsync()
        {
            var you = GetYouFabric();

            IChannelPOCO res = await you.GetChannelNetAsync("UCE27j85FZ8-aZOn6D8vWMWg");

            Assert.AreEqual(res.Title, "Vlad RT");
        }

        [TestMethod]
        public async Task GetChannelPlaylistNetAsync()
        {
            var you = GetYouFabric();

            IEnumerable<IPlaylistPOCO> res = await you.GetChannelPlaylistsNetAsync("UCq9B1wrqZKwucNkjHnUW39A");

            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetChannelRelatedPlaylistsNetAsync()
        {
            var you = GetYouFabric();
            IEnumerable<IPlaylistPOCO> lst = await you.GetChannelRelatedPlaylistsNetAsync("UC0lT9K8Wfuc1KPqm6YjRf1A");
            Assert.AreEqual(lst.Count(), 3);
        }

        [TestMethod]
        public async Task GetListVideoByIdsAsync()
        {
            var you = GetYouFabric();

            var lst = new List<string> { "-wA6Qj4oF2E" };

            IEnumerable<IVideoItemPOCO> res = await you.GetVideosListByIdsAsync(lst);

            Assert.AreEqual(res.Count(), 1);
        }

        [TestMethod]
        public async Task GetPlaylistItemsCountNetAsync()
        {
            var you = GetYouFabric();

            int res = await you.GetPlaylistItemsCountNetAsync("UU0lT9K8Wfuc1KPqm6YjRf1A");

            int res2 = await you.GetChannelItemsCountNetAsync("UC0lT9K8Wfuc1KPqm6YjRf1A");

            Assert.IsTrue(res == res2);
        }

        [TestMethod]
        public async Task GetPlaylistItemsIdsListNetAsync()
        {
            var you = GetYouFabric();

            IEnumerable<string> res = await you.GetPlaylistItemsIdsListNetAsync("PLt2cGgt6G8WrItA7KTI5m6EFniMfphWJC");

            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetPlaylistItemsNetAsync()
        {
            var you = GetYouFabric();

            IEnumerable<IVideoItemPOCO> lst = await you.GetPlaylistItemsNetAsync("UU0lT9K8Wfuc1KPqm6YjRf1A");

            Assert.IsTrue(lst.Any());
        }

        [TestMethod]
        public async Task GetPlaylistNetAsync()
        {
            var you = GetYouFabric();

            IPlaylistPOCO res = await YouTubeSite.GetPlaylistNetAsync("PLt2cGgt6G8WrItA7KTI5m6EFniMfphWJC");

            Assert.AreEqual(res.Title, "Creating Windows Services");

            Assert.AreEqual(res.SubTitle, "How to create, install and deploy windows services.");
        }

        [TestMethod]
        public async Task GetPopularItemsAsync()
        {
            var you = GetYouFabric();

            // var testindex = new[] { 1, 2, 25, 26, 27, 49, 50, 51 };
            var testindex = new[] { 2 };
            foreach (int i in testindex)
            {
                IEnumerable<IVideoItemPOCO> lst = await you.GetPopularItemsAsync("ru", i);
                Assert.AreEqual(lst.Count(), i);
            }
        }

        [TestMethod]
        public async Task GetRelatedChannelsByIdAsync()
        {
            var you = GetYouFabric();
            IEnumerable<IChannelPOCO> res = await you.GetRelatedChannelsByIdAsync("UCsNGRSN63gFoo5z6Oqv1A6A");
            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetVideoItemNetAsync()
        {
            var you = GetYouFabric();

            IVideoItemPOCO res = await you.GetVideoItemNetAsync("9bZkp7q19f0"); // lHgIpxQac3w

            Assert.AreEqual(res.Title, "Metallica — Unforgiven (FDM edition)");
        }

        [TestMethod]
        public async Task GetVideoSubtitlesByIdAsync()
        {
            var you = GetYouFabric();
            IEnumerable<ISubtitlePOCO> res = await you.GetVideoSubtitlesByIdAsync("WaEcvDnbaIc");
            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task SearchItemsAsync()
        {
            var you = GetYouFabric();
            var testindex = new[] { 5 };
            foreach (int i in testindex)
            {
                IEnumerable<IVideoItemPOCO> lst = await you.SearchItemsAsync("russia", "RU", i);
                Assert.AreEqual(lst.Count(), i);
            }
        }

        private async void FillCred()
        {
            var cf = fabric.CreateCredFactory();
            ICredPOCO poco = await fabric.CreateSqLiteDatabase().GetCredAsync("youtube.com");
            cred = cf.CreateCred(poco);
        }

        private YouTubeSite GetYouFabric()
        {
            var you = fabric.CreateYouTubeSite();
            you.Cred = cred;
            return you;
        }

        #endregion
    }
}
