// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataAPI.POCO;
using Interfaces.Models;

namespace DataAPI.Trackers
{
    public class RutrackerSite : CommonTracker
    {
        #region Fields

        private ICred cred;

        #endregion

        #region IRutrackerSite Members

        public ICred Cred
        {
            get
            {
                return cred;
            }
            set
            {
                cred = value;
                if (cred == null)
                {
                    return;
                }
                hostUrl = string.Format("http://{0}", cred.SiteAdress);
                _indexUrl = string.Format("{0}/index.php", hostUrl);
                _loginUrl = string.Format("http://login.{0}/forum/login.php", cred.SiteAdress);
                _profileUrl = string.Format("{0}/profile.php", hostUrl);
                _searchUrl = string.Format("{0}/forum/tracker.php?nm", hostUrl);
                _topicUrl = string.Format("{0}/forum/viewtopic.php?t", hostUrl);
                _userUrl = string.Format("{0}/forum/tracker.php?rid", hostUrl);
            }
        }

        public Task FillChannelNetAsync(IChannel channel)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<VideoItemPOCO>> GetChannelItemsAsync(IChannel channel, int maxresult)
        {
            throw new NotImplementedException();
        }

        public async Task<CookieContainer> GetCookieNetAsync(IChannel channel)
        {
            if (string.IsNullOrEmpty(Cred.Login) || string.IsNullOrEmpty(Cred.Pass))
            {
                throw new Exception("Please, set login and password");
            }

            var cc = new CookieContainer();
            var req = (HttpWebRequest)WebRequest.Create(_loginUrl);
            req.CookieContainer = cc;
            req.Method = WebRequestMethods.Http.Post;
            req.Host = "login." + Cred.SiteAdress;
            req.KeepAlive = true;
            string postData = string.Format("login_username={0}&login_password={1}&login=%C2%F5%EE%E4",
                Uri.EscapeDataString(Cred.Login),
                Uri.EscapeDataString(Cred.Pass));
            byte[] data = Encoding.ASCII.GetBytes(postData);
            req.ContentLength = data.Length;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Headers.Add("Cache-Control", "max-age=0");
            req.Headers.Add("Origin", hostUrl);
            req.Headers.Add("Accept-Language", "en-US,en;q=0.8");
            req.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
            req.Headers.Add("DNT", "1");
            req.Referer = _indexUrl;

            using (Stream stream = await req.GetRequestStreamAsync())
            {
                await stream.WriteAsync(data, 0, data.Length);
            }

            WebResponse resp = await req.GetResponseAsync();

            var res = (HttpWebResponse)resp;
            if (res.StatusCode == HttpStatusCode.OK)
            {
                cc.Add(res.Cookies);
            }
            return cc;
        }

        public Task<ChannelPOCO> GetChannelNetAsync(string channelID)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
