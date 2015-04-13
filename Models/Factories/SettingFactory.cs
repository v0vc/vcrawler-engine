using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Models.BO;

namespace Models.Factories
{
    class SettingFactory :ISettingFactory
    {
        public ISetting CreateSetting()
        {
            return new Setting();
        }

        public async Task<ISetting> GetSettingDbAsync(string key)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var fbres = await fb.GetSettingAsync(key);
                return new Setting(fbres);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertSettingAsync(Setting setting)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.InsertSettingAsync(setting);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteSettingAsync(string key)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.DeleteSettingAsync(key);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateSettingAsync(string key, string newvalue)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.UpdateSettingAsync(key, newvalue);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
