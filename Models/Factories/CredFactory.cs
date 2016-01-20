// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Threading.Tasks;
using DataAPI.Database;
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
            var cred = new Cred()
            {
                SiteAdress = poco.Site, 
                Login = poco.Login, 
                Pass = poco.Pass, 
                Cookie = poco.Cookie, 
                Expired = poco.Expired, 
                Autorization = poco.Autorization, 
                Site = EnumHelper.GetValueFromDescription<SiteType>(poco.Site)
            };
            return cred;
        }

        public static async Task DeleteCredAsync(string site)
        {
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
            try
            {
                await fb.DeleteCredAsync(site);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task InsertCredAsync(ICred cred)
        {
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
            try
            {
                await fb.InsertCredAsync(cred);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task UpdateAutorizationAsync(string site, short autorize)
        {
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
            try
            {
                await fb.UpdateAutorizationAsync(site, autorize);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task UpdateLoginAsync(string site, string newlogin)
        {
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
            try
            {
                await fb.UpdateLoginAsync(site, newlogin);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task UpdatePasswordAsync(string site, string newpassword)
        {
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
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
    }
}
