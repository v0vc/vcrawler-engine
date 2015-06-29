using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace TestAPI
{
    [TestClass]
    public class TapochekSiteTest
    {
        private const string Credsite = "tapochek.net";

        private readonly ICommonFactory _fabric;

        public TapochekSiteTest()
        {
            _fabric = IoC.Container.Kernel.Get<ICommonFactory>();
        }

        [TestMethod]
        public async Task GetChannelCookieNetAsync()
        {
            var cf = _fabric.CreateCredFactory();
            var cred = await cf.GetCredDbAsync(Credsite);

            var tp = _fabric.CreateTapochekSite();
            var cookie = await tp.GetCookieNetAsync(cred);
            Assert.IsTrue(cookie.Count > 0);
        }

        [TestMethod]
        public async Task StoreCookiesAsync()
        {
            var cf = _fabric.CreateCredFactory();
            var cred = await cf.GetCredDbAsync(Credsite);

            var tp = _fabric.CreateTapochekSite();
            var cookie = await tp.GetCookieNetAsync(cred);

            var chf = _fabric.CreateChannelFactory();
            var ch = chf.CreateChannel();
            ch.Site = cred.Site;
            ch.ChannelCookies = cookie;

            Task t = ch.StoreCookiesAsync();
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public async Task FillChannelCookieDbAsync()
        {
            var cf = _fabric.CreateCredFactory();
            var cred = await cf.GetCredDbAsync(Credsite);

            var chf = _fabric.CreateChannelFactory();
            var ch = chf.CreateChannel();
            ch.Site = cred.Site;

            Task t = ch.FillChannelCookieDbAsync();
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public async Task GetChannelItemsAsync()
        {
            var cf = _fabric.CreateCredFactory();
            var cred = await cf.GetCredDbAsync(Credsite);

            var tp = _fabric.CreateTapochekSite();

            var chf = _fabric.CreateChannelFactory();
            var ch = chf.CreateChannel();
            ch.Site = cred.Site;
            ch.ID = "27253";

            //if (cred.Expired <= DateTime.Now)
            //{
            //    await ch.FillChannelCookieNetAsync();
            //    await ch.StoreCookiesAsync();
            //}
            //else
            //{
                await ch.FillChannelCookieDbAsync();
            //}

            Task t = tp.GetChannelItemsAsync(ch, 0);
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public async Task GetChannelNetAsync()
        {
            var cf = _fabric.CreateCredFactory();
            var cred = await cf.GetCredDbAsync(Credsite);

            var tp = _fabric.CreateTapochekSite();

            var chf = _fabric.CreateChannelFactory();
            var ch = chf.CreateChannel();
            ch.Site = cred.Site;
            ch.ID = "27253";
            await ch.FillChannelCookieDbAsync();

            Task t = tp.GetChannelNetAsync(ch.ChannelCookies, ch.ID);
            Assert.IsTrue(!t.IsFaulted);
        }
    }
}
