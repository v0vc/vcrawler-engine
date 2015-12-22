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
using DataAPI.POCO;
using DataAPI.Trackers;
using Extensions.Helpers;
using Interfaces.Enums;
using Interfaces.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Models.BO.Channels;
using Models.Factories;

namespace TestAPI
{
    [TestClass]
    public class TapochekSiteTest
    {
        #region Static and Readonly Fields

        private readonly CommonFactory fabric;

        #endregion

        #region Fields

        private ICred cred;
        private TapochekSite tf;

        #endregion

        #region Constructors

        public TapochekSiteTest()
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
        public void FillChannelCookieDbAsync()
        {
            var chf = fabric.CreateChannelFactory();
            var ch = chf.CreateChannel(SiteType.Tapochek) as YouChannel;
            if (ch == null)
            {
                return;
            }
            ch.FillChannelCookieDb();
            Assert.IsTrue(ch.ChannelCookies.Count > 0);
        }

        [TestMethod]
        public async Task FillChannelNetAsync()
        {
            var chf = fabric.CreateChannelFactory();
            var ch = chf.CreateChannel(SiteType.Tapochek) as YouChannel;
            if (ch == null)
            {
                return;
            }
            ch.ID = "27253";
            ch.FillChannelCookieDb();
            await tf.FillChannelNetAsync(ch);
            Assert.IsTrue(ch.ChannelItems.Any());
        }

        [TestMethod]
        public async Task GetChannelCookieNetAsync()
        {
            var chf = fabric.CreateChannelFactory();
            IChannel ch = chf.CreateChannel(SiteType.Tapochek);
            CookieContainer cookie = await tf.GetCookieNetAsync(ch);
            Assert.IsTrue(cookie.Count > 0);
        }

        [TestMethod]
        public async Task GetChannelItemsAsync()
        {
            var chf = fabric.CreateChannelFactory();
            var ch = chf.CreateChannel(SiteType.Tapochek) as YouChannel;
            if (ch == null)
            {
                return;
            }
            ch.ID = "27253";
            ch.FillChannelCookieDb();
            if (ch.ChannelCookies == null)
            {
                await ch.FillChannelCookieNetAsync();
                //ch.StoreCookies();
            }
            IEnumerable<VideoItemPOCO> t = (await tf.GetChannelItemsAsync(ch, 0)).ToList();
            if (!t.Any())
            {
                await ch.FillChannelCookieNetAsync();
                //ch.StoreCookies();
                t = (await tf.GetChannelItemsAsync(ch, 0)).ToList();
            }
            Assert.IsTrue(t.Any());
        }

        [TestMethod]
        public async Task StoreCookiesAsync()
        {
            var chf = fabric.CreateChannelFactory();
            var ch = chf.CreateChannel(SiteType.Tapochek) as YouChannel;
            if (ch == null)
            {
                return;
            }
            CookieContainer cookie = await tf.GetCookieNetAsync(ch);
            ch.ChannelCookies = cookie;

            //ch.StoreCookies();
            var c = fabric.CreateSqLiteDatabase();
            if (c.FileBase.DirectoryName != null)
            {
                var folder = new DirectoryInfo(Path.Combine(c.FileBase.DirectoryName, "Cookie"));
                var fn = new FileInfo(Path.Combine(folder.FullName, EnumHelper.GetAttributeOfType(ch.Site)));
                Assert.IsTrue(fn.Exists);
            }
        }

        private async void FillCred()
        {
            cred = await fabric.CreateCredFactory().GetCredDbAsync(SiteType.Tapochek);
            tf = fabric.CreateTapochekSite();
            tf.Cred = cred;
        }

        #endregion
    }
}
