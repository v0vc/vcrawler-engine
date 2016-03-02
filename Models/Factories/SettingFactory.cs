// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Threading.Tasks;
using DataAPI.Database;
using DataAPI.POCO;
using Interfaces.Models;
using Models.BO;

namespace Models.Factories
{
    public static class SettingFactory
    {
        private static readonly SqLiteDatabase db = CommonFactory.CreateSqLiteDatabase();

        #region Static Methods

        public static ISetting CreateSetting()
        {
            return new Setting();
        }

        public static async Task<ISetting> GetSettingDbAsync(string key)
        {
            // var fb = ServiceLocator.SqLiteDatabase;
            // SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();

            try
            {
                SettingPOCO fbres = await db.GetSettingAsync(key).ConfigureAwait(false);
                ISetting set = CreateSetting(fbres);
                return set;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static ISetting CreateSetting(SettingPOCO poco)
        {
            var set = new Setting { Key = poco.Key, Value = poco.Value };
            return set;
        }

        #endregion
    }
}
