// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Interfaces.API;
using Interfaces.Enums;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace TestAPI
{
    [TestClass]
    public class TapochekSiteTest
    {
        #region Static and Readonly Fields

        private readonly ICommonFactory _fabric;

        #endregion

        #region Constructors

        public TapochekSiteTest()
        {
            _fabric = Container.Kernel.Get<ICommonFactory>();
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
            ITapochekSite tp = _fabric.CreateTapochekSite();
            IChannelFactory chf = _fabric.CreateChannelFactory();
            IChannel ch = chf.CreateChannel(SiteType.Tapochek);
            ch.ID = "27253";
            ch.FillChannelCookieDb();
            await tp.FillChannelNetAsync(ch);
            Assert.IsTrue(ch.ChannelItems.Any());
        }

        [TestMethod]
        public async Task GetChannelCookieNetAsync()
        {
            IChannelFactory chf = _fabric.CreateChannelFactory();
            IChannel ch = chf.CreateChannel(SiteType.Tapochek);
            ICred cred = await ch.GetChannelCredentialsAsync();
            ITapochekSite tp = _fabric.CreateTapochekSite();
            CookieContainer cookie = await tp.GetCookieNetAsync(ch);
            Assert.IsTrue(cookie.Count > 0);
        }

        [TestMethod]
        public async Task GetChannelItemsAsync()
        {
            IChannelFactory chf = _fabric.CreateChannelFactory();
            IChannel ch = chf.CreateChannel(SiteType.Tapochek);
            ITapochekSite tp = _fabric.CreateTapochekSite();
            ch.ID = "27253";
            ch.FillChannelCookieDb();
            if (ch.ChannelCookies == null)
            {
                await ch.FillChannelCookieNetAsync();
                ch.StoreCookies();
            }
            IEnumerable<IVideoItemPOCO> t = (await tp.GetChannelItemsAsync(ch, 0)).ToList();
            if (!t.Any())
            {
                await ch.FillChannelCookieNetAsync();
                ch.StoreCookies();
                t = (await tp.GetChannelItemsAsync(ch, 0)).ToList();
            }
            Assert.IsTrue(t.Any());
        }

        [TestMethod]
        public async Task StoreCookiesAsync()
        {
            IChannelFactory chf = _fabric.CreateChannelFactory();
            IChannel ch = chf.CreateChannel(SiteType.Tapochek);
            ICred cred = await ch.GetChannelCredentialsAsync();
            ITapochekSite tp = _fabric.CreateTapochekSite();
            CookieContainer cookie = await tp.GetCookieNetAsync(ch);
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

        #endregion
    }
}
