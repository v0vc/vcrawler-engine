using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SitesAPI.Videos;

namespace TestAPI
{
    [TestClass]
    public class YouTubeSiteTest
    {
        [TestMethod]
        public async Task SearchItemsAsync()
        {
            var you = new Mock<YouTubeSite>();
            var testindex = new[] {51};
            foreach (int i in testindex)
            {
                var lst = await you.Object.SearchItemsAsync("russia", i);
                Assert.AreEqual(lst.Count, i);
            }
        }

        [TestMethod]
        public async Task GetPopularItemsAsync()
        {
            var you = new Mock<YouTubeSite>();
            //var testindex = new[] { 1, 2, 25, 26, 27, 49, 50, 51 };
            var testindex = new[] { 1 };
            foreach (int i in testindex)
            {
                var lst = await you.Object.GetPopularItemsAsync("RU", i);
                Assert.AreEqual(lst.Count, i);
            }
        }

        [TestMethod]
        public async Task GetChannelItemsAsync()
        {
            var you = new Mock<YouTubeSite>();

            var lst = await you.Object.GetChannelItemsAsync("shupaltse666");

            Assert.IsTrue(lst.Any());
        }

        [TestMethod]
        public async Task GetPlaylistNetAsync()
        {
            var you = new Mock<YouTubeSite>();

            var res = await you.Object.GetPlaylistNetAsync("PLt2cGgt6G8WrItA7KTI5m6EFniMfphWJC");

            Assert.AreEqual(res.Title, "Creating Windows Services");

            Assert.AreEqual(res.SubTitle, "How to create, install and deploy windows services.");
        }

        [TestMethod]
        public async Task GetPlaylistItemsNetAsync()
        {
            var you = new Mock<YouTubeSite>();

            var res = await you.Object.GetPlaylistNetAsync("PLt2cGgt6G8WrItA7KTI5m6EFniMfphWJC");

            var lst = await you.Object.GetPlaylistItemsNetAsync(res.Link);

            Assert.IsTrue(lst.Any());
        }

        [TestMethod]
        public async Task GetChannelPlaylistNetAsync()
        {
            var you = new Mock<YouTubeSite>();

            var res = await you.Object.GetChannelPlaylistNetAsync("BablioBr");

            Assert.IsTrue(res.Any());
        }

        [TestMethod]
        public async Task GetChannelNetAsync()
        {
            var you = new Mock<YouTubeSite>();

            var res = await you.Object.GetChannelNetAsync("BablioBr");

            Assert.AreEqual(res.Title, "Fabio Scopel");
        }

        [TestMethod]
        public async Task GetVideoItemNetAsync()
        {
            var you = new Mock<YouTubeSite>();

            var res = await you.Object.GetVideoItemNetAsync("lHgIpxQac3w");

            Assert.AreEqual(res.Title, "Metallica — Unforgiven (FDM edition)");
        }
    }
}
