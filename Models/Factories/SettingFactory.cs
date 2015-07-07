using System;
using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Models.BO;

namespace Models.Factories
{
    public class SettingFactory : ISettingFactory
    {
        private readonly ICommonFactory _c;

        public SettingFactory(ICommonFactory c)
        {
            _c = c;
        }

        public ISetting CreateSetting()
        {
            return new Setting(this);
        }

        public async Task<ISetting> GetSettingDbAsync(string key)
        {
            var fb = _c.CreateSqLiteDatabase();

            // var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var fbres = await fb.GetSettingAsync(key);
                return new Setting(fbres, _c.CreateSettingFactory());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertSettingAsync(Setting setting)
        {
            var fb = _c.CreateSqLiteDatabase();
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
            var fb = _c.CreateSqLiteDatabase();
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
            var fb = _c.CreateSqLiteDatabase();
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
