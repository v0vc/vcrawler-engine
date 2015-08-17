using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interfaces.Factories;
using IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace TestAPI
{
    [TestClass]
    public class YouTubeSiteTest
    {
        private readonly ICommonFactory _fabric;

        public YouTubeSiteTest()
        {
            _fabric = Container.Kernel.Get<ICommonFactory>();
        }

        [TestMethod]
        public async Task SearchItemsAsync()
        {
            var you = _fabric.CreateYouTubeSite();
            var testindex = new[] {5};
            foreach (int i in testindex)
            {
                var lst = await you.SearchItemsAsync("russia", "RU", i);
                Assert.AreEqual(lst.Count(), i);
            }
        }

        [TestMethod]
        public async Task GetPopularItemsAsync()
        {
            var you = _fabric.CreateYouTubeSite();

            // var testindex = new[] { 1, 2, 25, 26, 27, 49, 50, 51 };
            var testindex = new[] { 2 };
            foreach (int i in testindex)
            {
                var lst = await you.GetPopularItemsAsync("ru", i);
                Assert.AreEqual(lst.Count(), i);
            }
        }

        [TestMethod]
        public async Task GetChannelItemsAsync()
        {
            var you = _fabric.CreateYouTubeSite();

            // UCQoZVSaWvaJN046F-8SmyPg
            // UCq9B1wrqZKwucNkjHnUW39A
            var lst = await you.GetChannelItemsAsync("UCQoZVSaWvaJN046F-8SmyPg", 5);

            Assert.AreEqual(lst.Count(), 5);
        }

        [TestMethod]
        public async Task GetPlaylistNetAsync()
        {
            var you = _fabric.CreateYouTubeSite();

            var res = await you.GetPlaylistNetAsync("PLt2cGgt6G8WrItA7KTI5m6EFniMfphWJC");

            Assert.AreEqual(res.Title, "Creating Windows Services");

            Assert.AreEqual(res.SubTitle, "How to create, install and deploy windows services.");
        }

        [TestMethod]
        public async Task GetPlaylistItemsNetAsync()
        {
            var you = _fabric.CreateYouTubeSite();

            var lst = await you.GetPlaylistItemsNetAsync("PLiCpP_44QZByvNe1h9hGLlOXIXRwnz5l3");

            Assert.IsTrue(lst.Any());
        }

        [TestMethod]
        public async Task GetChannelPlaylistNetAsync()
        {
            var you = _fabric.CreateYouTubeSite();

            var res = await you.GetChannelPlaylistNetAsync("UCq9B1wrqZKwucNkjHnUW39A");

            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetChannelNetAsync()
        {
            var you = _fabric.CreateYouTubeSite();

            var res = await you.GetChannelNetAsync("UCE27j85FZ8-aZOn6D8vWMWg");

            Assert.AreEqual(res.Title, "Vlad RT");
        }

        [TestMethod]
        public async Task GetVideoItemNetAsync()
        {
            var you = _fabric.CreateYouTubeSite();

            var res = await you.GetVideoItemNetAsync("9bZkp7q19f0"); // lHgIpxQac3w

            Assert.AreEqual(res.Title, "Metallica — Unforgiven (FDM edition)");
        }

        [TestMethod]
        public async Task GetChannelItemsCountNetAsync()
        {
            var you = _fabric.CreateYouTubeSite();

            var res = await you.GetChannelItemsCountNetAsync("UCE27j85FZ8-aZOn6D8vWMWg");

            Assert.AreEqual(res, 8);
        }

        [TestMethod]
        public async Task GetChannelItemsIdsListNetAsync()
        {
            var you = _fabric.CreateYouTubeSite();

            var res = await you.GetChannelItemsIdsListNetAsync("UCE27j85FZ8-aZOn6D8vWMWg", 5);

            Assert.AreEqual(res.Count(), 5);
        }

        [TestMethod]
        public async Task GetPlaylistItemsIdsListNetAsync()
        {
            var you = _fabric.CreateYouTubeSite();

            var res = await you.GetPlaylistItemsIdsListNetAsync("PLt2cGgt6G8WrItA7KTI5m6EFniMfphWJC");

            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetChannelIdByUserNameNetAsync()
        {
            var you = _fabric.CreateYouTubeSite();

            var res = await you.GetChannelIdByUserNameNetAsync("mcmbmirussian");

            Assert.AreEqual(res, "UCH0miwnqCojki-ado_lLI5A");
        }

        [TestMethod]
        public async Task GetListVideoByIdsAsync()
        {
            var you = _fabric.CreateYouTubeSite();

            var lst = new List<string> { "-wA6Qj4oF2E" };

            var res = await you.GetVideosListByIdsAsync(lst);

            Assert.AreEqual(res.Count(), 1);
        }

        [TestMethod]
        public async Task GetRelatedChannelsByIdAsync()
        {
            var you = _fabric.CreateYouTubeSite();
            var res = await you.GetRelatedChannelsByIdAsync("UCsNGRSN63gFoo5z6Oqv1A6A");
            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetVideoSubtitlesByIdAsync()
        {
            var you = _fabric.CreateYouTubeSite();
            var res = await you.GetVideoSubtitlesByIdAsync("WaEcvDnbaIc");
            Assert.IsTrue(res.Any());
        }
    }
}
