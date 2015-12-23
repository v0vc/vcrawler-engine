﻿// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
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
    public class SettingFactory
    {
        #region Static and Readonly Fields

        //private readonly CommonFactory commonFactory;

        //#endregion

        //#region Constructors

        //public SettingFactory(CommonFactory commonFactory)
        //{
        //    this.commonFactory = commonFactory;
        //}

        #endregion

        #region Methods

        public ISetting CreateSetting()
        {
            return new Setting(this);
        }

        public async Task DeleteSettingAsync(string key)
        {
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
            try
            {
                await fb.DeleteSettingAsync(key);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ISetting> GetSettingDbAsync(string key)
        {
            // var fb = ServiceLocator.SqLiteDatabase;
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
            SettingFactory sf = CommonFactory.CreateSettingFactory();

            try
            {
                SettingPOCO fbres = await fb.GetSettingAsync(key);
                ISetting set = sf.CreateSetting(fbres);
                return set;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertSettingAsync(Setting setting)
        {
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
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
            SqLiteDatabase fb = CommonFactory.CreateSqLiteDatabase();
            try
            {
                await fb.UpdateSettingAsync(key, newvalue);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private ISetting CreateSetting(SettingPOCO poco)
        {
            var set = new Setting(this) { Key = poco.Key, Value = poco.Value };
            return set;
        }

        #endregion
    }
}
