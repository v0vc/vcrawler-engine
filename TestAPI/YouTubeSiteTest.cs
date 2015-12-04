// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Interfaces.API;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestAPI
{
    [TestClass]
    public class YouTubeSiteTest
    {
        #region Static and Readonly Fields

        private readonly ICommonFactory fabric;

        #endregion

        #region Fields

        private ICred cred;

        #endregion

        #region Constructors

        public YouTubeSiteTest()
        {
            using (ILifetimeScope scope = Container.Kernel.BeginLifetimeScope())
            {
                fabric = scope.Resolve<ICommonFactory>();
            }
            FillCred();
        }

        #endregion

        #region Methods

        [TestMethod]
        public async Task GetChannelIdByUserNameNetAsync()
        {
            IYouTubeSite you = GetYouFabric();

            string res = await you.GetChannelIdByUserNameNetAsync("mcmbmirussian");

            Assert.AreEqual(res, "UCH0miwnqCojki-ado_lLI5A");
        }

        [TestMethod]
        public async Task GetChannelItemsAsync()
        {
            IYouTubeSite you = GetYouFabric();

            // UCQoZVSaWvaJN046F-8SmyPg
            // UCq9B1wrqZKwucNkjHnUW39A
            IEnumerable<IVideoItemPOCO> lst = await you.GetChannelItemsAsync("UCQoZVSaWvaJN046F-8SmyPg", 5);

            Assert.AreEqual(lst.Count(), 5);
        }

        [TestMethod]
        public async Task GetChannelItemsCountNetAsync()
        {
            IYouTubeSite you = GetYouFabric();

            int res = await you.GetChannelItemsCountNetAsync("UCE27j85FZ8-aZOn6D8vWMWg");

            Assert.AreEqual(res, 8);
        }

        [TestMethod]
        public async Task GetChannelItemsIdsListNetAsync()
        {
            IYouTubeSite you = GetYouFabric();

            IEnumerable<string> res = await you.GetChannelItemsIdsListNetAsync("UCE27j85FZ8-aZOn6D8vWMWg", 5);

            Assert.AreEqual(res.Count(), 5);
        }

        [TestMethod]
        public async Task GetChannelNetAsync()
        {
            IYouTubeSite you = GetYouFabric();

            IChannelPOCO res = await you.GetChannelNetAsync("UCE27j85FZ8-aZOn6D8vWMWg");

            Assert.AreEqual(res.Title, "Vlad RT");
        }

        [TestMethod]
        public async Task GetChannelPlaylistNetAsync()
        {
            IYouTubeSite you = GetYouFabric();

            IEnumerable<IPlaylistPOCO> res = await you.GetChannelPlaylistsNetAsync("UCq9B1wrqZKwucNkjHnUW39A");

            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetChannelRelatedPlaylistsNetAsync()
        {
            IYouTubeSite you = GetYouFabric();
            IEnumerable<IPlaylistPOCO> lst = await you.GetChannelRelatedPlaylistsNetAsync("UC0lT9K8Wfuc1KPqm6YjRf1A");
            Assert.AreEqual(lst.Count(), 3);
        }

        [TestMethod]
        public async Task GetListVideoByIdsAsync()
        {
            IYouTubeSite you = GetYouFabric();

            var lst = new List<string> { "-wA6Qj4oF2E" };

            IEnumerable<IVideoItemPOCO> res = await you.GetVideosListByIdsAsync(lst);

            Assert.AreEqual(res.Count(), 1);
        }

        [TestMethod]
        public async Task GetPlaylistItemsIdsListNetAsync()
        {
            IYouTubeSite you = GetYouFabric();

            IEnumerable<string> res = await you.GetPlaylistItemsIdsListNetAsync("PLt2cGgt6G8WrItA7KTI5m6EFniMfphWJC");

            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetPlaylistItemsCountNetAsync()
        {
            IYouTubeSite you = GetYouFabric();

            int res = await you.GetPlaylistItemsCountNetAsync("UU0lT9K8Wfuc1KPqm6YjRf1A");

            int res2 = await you.GetChannelItemsCountBySearchNetAsync("UC0lT9K8Wfuc1KPqm6YjRf1A");

            Assert.IsTrue(res == res2);
        }

        [TestMethod]
        public async Task GetPlaylistItemsNetAsync()
        {
            IYouTubeSite you = GetYouFabric();

            IEnumerable<IVideoItemPOCO> lst = await you.GetPlaylistItemsNetAsync("UU0lT9K8Wfuc1KPqm6YjRf1A");

            Assert.IsTrue(lst.Any());
        }

        [TestMethod]
        public async Task GetPlaylistNetAsync()
        {
            IYouTubeSite you = GetYouFabric();

            IPlaylistPOCO res = await you.GetPlaylistNetAsync("PLt2cGgt6G8WrItA7KTI5m6EFniMfphWJC");

            Assert.AreEqual(res.Title, "Creating Windows Services");

            Assert.AreEqual(res.SubTitle, "How to create, install and deploy windows services.");
        }

        [TestMethod]
        public async Task GetPopularItemsAsync()
        {
            IYouTubeSite you = GetYouFabric();

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
            IYouTubeSite you = GetYouFabric();
            IEnumerable<IChannelPOCO> res = await you.GetRelatedChannelsByIdAsync("UCsNGRSN63gFoo5z6Oqv1A6A");
            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetVideoItemNetAsync()
        {
            IYouTubeSite you = GetYouFabric();

            IVideoItemPOCO res = await you.GetVideoItemNetAsync("9bZkp7q19f0"); // lHgIpxQac3w

            Assert.AreEqual(res.Title, "Metallica — Unforgiven (FDM edition)");
        }

        [TestMethod]
        public async Task GetVideoSubtitlesByIdAsync()
        {
            IYouTubeSite you = GetYouFabric();
            IEnumerable<ISubtitlePOCO> res = await you.GetVideoSubtitlesByIdAsync("WaEcvDnbaIc");
            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task SearchItemsAsync()
        {
            IYouTubeSite you = GetYouFabric();
            var testindex = new[] { 5 };
            foreach (int i in testindex)
            {
                IEnumerable<IVideoItemPOCO> lst = await you.SearchItemsAsync("russia", "RU", i);
                Assert.AreEqual(lst.Count(), i);
            }
        }

        private async void FillCred()
        {
            ICredFactory cf = fabric.CreateCredFactory();
            ICredPOCO poco = await fabric.CreateSqLiteDatabase().GetCredAsync("youtube.com");
            cred = cf.CreateCred(poco);
        }

        private IYouTubeSite GetYouFabric()
        {
            IYouTubeSite you = fabric.CreateYouTubeSite();
            you.Cred = cred;
            return you;
        }

        #endregion
    }
}
