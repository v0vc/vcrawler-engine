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
    }
}
