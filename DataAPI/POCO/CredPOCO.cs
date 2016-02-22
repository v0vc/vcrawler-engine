// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;

namespace DataAPI.POCO
{
    public class CredPOCO
    {
        #region Constructors

        public CredPOCO(string site,
            string login,
            string pass,
            short autorize = 0,
            DateTime expired = default(DateTime),
            string cookie = null)
        {
            Site = site;
            Login = login;
            Pass = pass;
            Cookie = cookie;
            Expired = expired;
            Autorization = autorize;
        }

        #endregion

        #region Properties

        public short Autorization { get; private set; }
        public string Cookie { get; private set; }
        public DateTime Expired { get; private set; }
        public string Login { get; private set; }
        public string Pass { get; private set; }
        public string Site { get; private set; }

        #endregion
    }
}
