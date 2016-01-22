// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using DataAPI.POCO;
using Extensions.Helpers;
using Interfaces.Enums;
using Interfaces.Models;
using Models.BO;

namespace Models.Factories
{
    public static class CredFactory
    {
        #region Static Methods

        public static ICred CreateCred()
        {
            return new Cred();
        }

        public static ICred CreateCred(CredPOCO poco)
        {
            var cred = new Cred
            {
                Login = poco.Login,
                Pass = poco.Pass,
                Cookie = poco.Cookie,
                Expired = poco.Expired,
                Autorization = poco.Autorization,
                Site = EnumHelper.GetValueFromDescription<SiteType>(poco.Site)
            };
            return cred;
        }

        #endregion
    }
}
