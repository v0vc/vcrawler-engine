// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Threading.Tasks;
using Extensions;
using Interfaces.Enums;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO;

namespace Models.Factories
{
    public class CredFactory
    {
        #region Static and Readonly Fields

        private readonly CommonFactory commonFactory;

        #endregion

        #region Constructors

        public CredFactory(CommonFactory commonFactory)
        {
            this.commonFactory = commonFactory;
        }

        #endregion

        #region Methods

        public async Task DeleteCredAsync(string site)
        {
            var fb = commonFactory.CreateSqLiteDatabase();
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
            var fb = commonFactory.CreateSqLiteDatabase();
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
            var fb = commonFactory.CreateSqLiteDatabase();
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
            var fb = commonFactory.CreateSqLiteDatabase();
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
            var fb = commonFactory.CreateSqLiteDatabase();
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
            var fb = commonFactory.CreateSqLiteDatabase();
            var cf = commonFactory.CreateCredFactory();

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
