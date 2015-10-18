// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Autofac;
using Interfaces.API;
using Interfaces.Enums;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestAPI
{
    [TestClass]
    public class TapochekSiteTest
    {
        #region Static and Readonly Fields

        private readonly ICommonFactory _fabric;

        #endregion

        #region Fields

        private ITapochekSite _tf;
        private ICred cred;

        #endregion

        #region Constructors

        public TapochekSiteTest()
        {
            using (var scope = Container.Kernel.BeginLifetimeScope())
            {
                _fabric = scope.Resolve<ICommonFactory>();
            }
            FillCred();
        }

        #endregion

        #region Methods

        [TestMethod]
        public void FillChannelCookieDbAsync()
        {
            IChannelFactory chf = _fabric.CreateChannelFactory();
            IChannel ch = chf.CreateChannel(SiteType.Tapochek);
            ch.FillChannelCookieDb();
            Assert.IsTrue(ch.ChannelCookies.Count > 0);
        }

        [TestMethod]
        public async Task FillChannelNetAsync()
        {
            IChannelFactory chf = _fabric.CreateChannelFactory();
            IChannel ch = chf.CreateChannel(SiteType.Tapochek);
            ch.ID = "27253";
            ch.FillChannelCookieDb();
            await _tf.FillChannelNetAsync(ch);
            Assert.IsTrue(ch.ChannelItems.Any());
        }

        [TestMethod]
        public async Task GetChannelCookieNetAsync()
        {
            IChannelFactory chf = _fabric.CreateChannelFactory();
            IChannel ch = chf.CreateChannel(SiteType.Tapochek);
            CookieContainer cookie = await _tf.GetCookieNetAsync(ch);
            Assert.IsTrue(cookie.Count > 0);
        }

        [TestMethod]
        public async Task GetChannelItemsAsync()
        {
            IChannelFactory chf = _fabric.CreateChannelFactory();
            IChannel ch = chf.CreateChannel(SiteType.Tapochek);
            ch.ID = "27253";
            ch.FillChannelCookieDb();
            if (ch.ChannelCookies == null)
            {
                await ch.FillChannelCookieNetAsync();
                ch.StoreCookies();
            }
            IEnumerable<IVideoItemPOCO> t = (await _tf.GetChannelItemsAsync(ch, 0)).ToList();
            if (!t.Any())
            {
                await ch.FillChannelCookieNetAsync();
                ch.StoreCookies();
                t = (await _tf.GetChannelItemsAsync(ch, 0)).ToList();
            }
            Assert.IsTrue(t.Any());
        }

        [TestMethod]
        public async Task StoreCookiesAsync()
        {
            IChannelFactory chf = _fabric.CreateChannelFactory();
            IChannel ch = chf.CreateChannel(SiteType.Tapochek);
            CookieContainer cookie = await _tf.GetCookieNetAsync(ch);
            ch.ChannelCookies = cookie;

            ch.StoreCookies();
            ISqLiteDatabase c = _fabric.CreateSqLiteDatabase();
            if (c.FileBase.DirectoryName != null)
            {
                var folder = new DirectoryInfo(Path.Combine(c.FileBase.DirectoryName, "Cookie"));
                var fn = new FileInfo(Path.Combine(folder.FullName, ch.SiteAdress));
                Assert.IsTrue(fn.Exists);
            }
        }

        private async void FillCred()
        {
            cred = await _fabric.CreateCredFactory().GetCredDbAsync(SiteType.Tapochek);
            _tf = _fabric.CreateTapochekSite();
            _tf.Cred = cred;
        }

        #endregion
    }
}
