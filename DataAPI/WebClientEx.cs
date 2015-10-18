// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Net;

namespace DataAPI
{
    public class WebClientEx : WebClient
    {
        #region Static and Readonly Fields

        private readonly CookieContainer container;

        #endregion

        #region Constructors

        public WebClientEx(CookieContainer container)
        {
            this.container = container;
        }

        #endregion

        #region Methods

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            if (request != null)
            {
                request.CookieContainer = container;
            }
            return r;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response == null)
            {
                return;
            }
            CookieCollection cookies = response.Cookies;
            container.Add(cookies);
        }

        #endregion
    }
}
