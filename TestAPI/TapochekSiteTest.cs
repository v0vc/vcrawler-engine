// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataAPI.Database;
using DataAPI.POCO;
using DataAPI.Trackers;
using Extensions.Helpers;
using Interfaces.Enums;
using Interfaces.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models.BO.Channels;
using Models.Factories;

namespace TestAPI
{
    [TestClass]
    public class TapochekSiteTest
    {
        #region Static and Readonly Fields

        private readonly TapochekSite tf;
        private readonly SqLiteDatabase db;

        #endregion

        #region Constructors

        public TapochekSiteTest()
        {
            tf = CommonFactory.CreateTapochekSite();
            db = CommonFactory.CreateSqLiteDatabase();
        }

        #endregion

        #region Methods

        [TestMethod]
        public void FillChannelCookieDbAsync()
        {
            IChannel ch = ChannelFactory.CreateChannel(SiteType.Tapochek);
            if (ch == null)
            {
                return;
            }
            ch.ChannelCookies = db.ReadCookies(ch.Site);
            Assert.IsTrue(ch.ChannelCookies.Count > 0);
        }

        [TestMethod]
        public async Task FillChannelNetAsync()
        {
            var ch = ChannelFactory.CreateChannel(SiteType.Tapochek) as YouChannel;
            if (ch == null)
            {
                return;
            }
            ch.ID = "27253";
            ch.ChannelCookies = db.ReadCookies(ch.Site);
            await tf.FillChannelNetAsync(ch).ConfigureAwait(false);
            Assert.IsTrue(ch.ChannelItems.Any());
        }

        [TestMethod]
        public async Task GetChannelCookieNetAsync()
        {
            IChannel ch = ChannelFactory.CreateChannel(SiteType.Tapochek);
            CookieContainer cookie = await tf.GetCookieNetAsync(ch).ConfigureAwait(false);
            Assert.IsTrue(cookie.Count > 0);
        }

        [TestMethod]
        public async Task GetChannelItemsAsync()
        {
            IChannel ch = ChannelFactory.CreateChannel(SiteType.Tapochek);
            if (ch == null)
            {
                return;
            }
            ch.ID = "27253";

            ch.ChannelCookies = db.ReadCookies(ch.Site);
            if (ch.ChannelCookies == null)
            {
                await ChannelFactory.FillChannelCookieNetAsync(ch).ConfigureAwait(false);
            }
            IEnumerable<VideoItemPOCO> t = (await tf.GetChannelItemsAsync(ch, 0).ConfigureAwait(false)).ToList();
            if (!t.Any())
            {
                await ChannelFactory.FillChannelCookieNetAsync(ch).ConfigureAwait(false);
                t = (await tf.GetChannelItemsAsync(ch, 0).ConfigureAwait(false)).ToList();
            }
            Assert.IsTrue(t.Any());
        }

        [TestMethod]
        public async Task StoreCookiesAsync()
        {
            var ch = ChannelFactory.CreateChannel(SiteType.Tapochek) as YouChannel;
            if (ch == null)
            {
                return;
            }
            CookieContainer cookie = await tf.GetCookieNetAsync(ch).ConfigureAwait(false);
            ch.ChannelCookies = cookie;

            // ch.StoreCookies();
           
            if (db.FileBase.DirectoryName != null)
            {
                var folder = new DirectoryInfo(Path.Combine(db.FileBase.DirectoryName, "Cookie"));
                var fn = new FileInfo(Path.Combine(folder.FullName, EnumHelper.GetAttributeOfType(ch.Site)));
                Assert.IsTrue(fn.Exists);
            }
        }

        #endregion
    }
}
