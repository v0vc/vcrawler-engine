// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Threading.Tasks;
using Interfaces.API;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO;

namespace Models.Factories
{
    public class SettingFactory : ISettingFactory
    {
        #region Static and Readonly Fields

        private readonly CommonFactory commonFactory;

        #endregion

        #region Constructors

        public SettingFactory(CommonFactory commonFactory)
        {
            this.commonFactory = commonFactory;
        }

        #endregion

        #region Methods

        public async Task DeleteSettingAsync(string key)
        {
            ISqLiteDatabase fb = commonFactory.CreateSqLiteDatabase();
            try
            {
                await fb.DeleteSettingAsync(key);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertSettingAsync(Setting setting)
        {
            ISqLiteDatabase fb = commonFactory.CreateSqLiteDatabase();
            try
            {
                await fb.InsertSettingAsync(setting);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateSettingAsync(string key, string newvalue)
        {
            ISqLiteDatabase fb = commonFactory.CreateSqLiteDatabase();
            try
            {
                await fb.UpdateSettingAsync(key, newvalue);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region ISettingFactory Members

        public ISetting CreateSetting()
        {
            return new Setting(this);
        }

        public ISetting CreateSetting(ISettingPOCO poco)
        {
            var set = new Setting(this) { Key = poco.Key, Value = poco.Value };
            return set;
        }

        public async Task<ISetting> GetSettingDbAsync(string key)
        {
            // var fb = ServiceLocator.SqLiteDatabase;
            ISqLiteDatabase fb = commonFactory.CreateSqLiteDatabase();
            ISettingFactory sf = commonFactory.CreateSettingFactory();

            try
            {
                ISettingPOCO fbres = await fb.GetSettingAsync(key);
                ISetting set = sf.CreateSetting(fbres);
                return set;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion
    }
}
