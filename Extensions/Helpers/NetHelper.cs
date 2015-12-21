// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Net;

namespace Extensions.Helpers
{
    public class NetHelper
    {
        #region Static Methods

        public static byte[] GetStreamFromUrl(string url)
        {
            byte[] imageData;

            using (var wc = new WebClient())
            {
                imageData = wc.DownloadData(url);
            }

            return imageData;
        }

        public static bool IsUrlExist(string url)
        {
            try
            {
                var request = WebRequest.Create(url) as HttpWebRequest;
                if (request != null)
                {
                    request.Method = "HEAD";
                    var response = request.GetResponse() as HttpWebResponse;
                    if (response != null)
                    {
                        response.Close();
                        return response.StatusCode == HttpStatusCode.OK;
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        #endregion
    }
}
