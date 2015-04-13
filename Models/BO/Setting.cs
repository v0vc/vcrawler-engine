using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Models;
using Interfaces.POCO;
using Models.Factories;

namespace Models.BO
{
    public class Setting :ISetting
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public Setting()
        {

        }
        public Setting(ISettingPOCO setting)
        {
            Key = setting.Key;
            Value = setting.Value;
        }

        public async Task InsertSettingAsync()
        {
            await ((SettingFactory) ServiceLocator.SettingFactory).InsertSettingAsync(this);
        }

        public async Task DeleteSettingAsync()
        {
            await ((SettingFactory) ServiceLocator.SettingFactory).DeleteSettingAsync(Key);
        }

        public async Task UpdateSettingAsync(string newvalue)
        {
            await ((SettingFactory) ServiceLocator.SettingFactory).UpdateSettingAsync(Key, newvalue);
        }
    }
}
