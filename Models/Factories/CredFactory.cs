// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Threading.Tasks;
using Extensions;
using Interfaces.API;
using Interfaces.Enums;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO;

namespace Models.Factories
{
    public class CredFactory : ICredFactory
    {
        #region Static and Readonly Fields

        private readonly ICommonFactory _c;

        #endregion

        #region Constructors

        public CredFactory(ICommonFactory c)
        {
            _c = c;
        }

        #endregion

        #region Methods

        public async Task DeleteCredAsync(string site)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.DeleteCredAsync(site);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertCredAsync(ICred cred)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.InsertCredAsync(cred);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateAutorizationAsync(string site, short autorize)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.UpdateAutorizationAsync(site, autorize);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateLoginAsync(string site, string newlogin)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.UpdateLoginAsync(site, newlogin);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdatePasswordAsync(string site, string newpassword)
        {
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.UpdatePasswordAsync(site, newpassword);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region ICredFactory Members

        public ICred CreateCred()
        {
            return new Cred(this);
        }

        public ICred CreateCred(ICredPOCO poco)
        {
            var cred = new Cred(this)
            {
                SiteAdress = poco.Site, 
                Login = poco.Login, 
                Pass = poco.Pass, 
                Cookie = poco.Cookie, 
                Expired = poco.Expired, 
                Autorization = poco.Autorization,
                Site = CommonExtensions.GetSiteType(poco.Site)
            };
            return cred;
        }

        public async Task<ICred> GetCredDbAsync(SiteType site)
        {
            // var fb = ServiceLocator.SqLiteDatabase;
            ISqLiteDatabase fb = _c.CreateSqLiteDatabase();
            ICredFactory cf = _c.CreateCredFactory();

            try
            {
                ICredPOCO poco = await fb.GetCredAsync(CommonExtensions.GetSiteAdress(site));
                ICred cred = cf.CreateCred(poco);
                return cred;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion
    }
}
