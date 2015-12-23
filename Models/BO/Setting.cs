// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Threading.Tasks;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO
{
    public class Setting : ISetting
    {
        #region ISetting Members

        public string Key { get; set; }
        public string Value { get; set; }

        public async Task DeleteSettingAsync()
        {
            await SettingFactory.DeleteSettingAsync(Key);
        }

        public async Task InsertSettingAsync()
        {
            // await ((SettingFactory) ServiceLocator.SettingFactory).InsertSettingAsync(this);
            await SettingFactory.InsertSettingAsync(this);
        }

        public async Task UpdateSettingAsync(string newvalue)
        {
            await SettingFactory.UpdateSettingAsync(Key, newvalue);
        }

        #endregion
    }
}
