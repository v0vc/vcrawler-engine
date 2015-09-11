// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Interfaces.API;
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
        #region Constants

        private const string Credsite = "tapochek.net";

        #endregion

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
        public async Task FillChannelCookieDbAsync()
        {
            ICredFactory cf = _fabric.CreateCredFactory();
            ICred cred = await cf.GetCredDbAsync(Credsite);

            IChannelFactory chf = _fabric.CreateChannelFactory();
            IChannel ch = chf.CreateChannel();
            ch.Site = cred.Site;

            ch.FillChannelCookieDb();
            Assert.IsTrue(ch.ChannelCookies.Count > 0);
        }

        [TestMethod]
        public async Task GetChannelCookieNetAsync()
        {
            ICredFactory cf = _fabric.CreateCredFactory();
            ICred cred = await cf.GetCredDbAsync(Credsite);

            ITapochekSite tp = _fabric.CreateTapochekSite();
            CookieContainer cookie = await tp.GetCookieNetAsync(cred);
            Assert.IsTrue(cookie.Count > 0);
        }

        [TestMethod]
        public async Task GetChannelItemsAsync()
        {
            ICredFactory cf = _fabric.CreateCredFactory();
            ICred cred = await cf.GetCredDbAsync(Credsite);

            ITapochekSite tp = _fabric.CreateTapochekSite();

            IChannelFactory chf = _fabric.CreateChannelFactory();
            IChannel ch = chf.CreateChannel();
            ch.Site = cred.Site;
            ch.ID = "27253";

            if (cred.Expired <= DateTime.Now)
            {
                await ch.FillChannelCookieNetAsync();
                ch.StoreCookies();
            }
            else
            {
                ch.FillChannelCookieDb();
            }

            // await ch.FillChannelCookieDbAsync();
            // await ch.FillChannelCookieNetAsync();
            // await ch.StoreCookiesAsync();
            IEnumerable<IVideoItemPOCO> t = await tp.GetChannelItemsAsync(ch, 0);
            Assert.IsTrue(t.Any());
        }

        [TestMethod]
        public async Task GetChannelNetAsync()
        {
            ICredFactory cf = _fabric.CreateCredFactory();
            ICred cred = await cf.GetCredDbAsync(Credsite);

            ITapochekSite tp = _fabric.CreateTapochekSite();

            IChannelFactory chf = _fabric.CreateChannelFactory();
            IChannel ch = chf.CreateChannel();
            ch.Site = cred.Site;
            ch.ID = "27253";
            ch.FillChannelCookieDb();

            Task t = tp.GetChannelNetAsync(ch.ChannelCookies, ch.ID);
            Assert.IsTrue(!t.IsFaulted);
        }

        [TestMethod]
        public async Task StoreCookiesAsync()
        {
            ICredFactory cf = _fabric.CreateCredFactory();
            ICred cred = await cf.GetCredDbAsync(Credsite);

            ITapochekSite tp = _fabric.CreateTapochekSite();
            CookieContainer cookie = await tp.GetCookieNetAsync(cred);

            IChannelFactory chf = _fabric.CreateChannelFactory();
            IChannel ch = chf.CreateChannel();
            ch.Site = cred.Site;
            ch.ChannelCookies = cookie;

            ch.StoreCookies();
            ISqLiteDatabase c = _fabric.CreateSqLiteDatabase();
            if (c.FileBase.DirectoryName != null)
            {
                var folder = new DirectoryInfo(Path.Combine(c.FileBase.DirectoryName, "Cookie"));
                var fn = new FileInfo(Path.Combine(folder.FullName, ch.Site));
                Assert.IsTrue(fn.Exists);
            }
        }

        #endregion
    }
}
