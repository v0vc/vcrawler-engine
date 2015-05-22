using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.Factories;

namespace Models.BO
{
    public class Setting :ISetting
    {
        private readonly SettingFactory _sf;
        public string Key { get; set; }
        public string Value { get; set; }

        public Setting(ISettingFactory sf)
        {
            _sf = sf as SettingFactory;
        }

        public Setting(ISettingPOCO setting, ISettingFactory sf)
        {
            _sf = sf as SettingFactory;
            Key = setting.Key;
            Value = setting.Value;
        }

        public async Task InsertSettingAsync()
        {
            await _sf.InsertSettingAsync(this);
            //await ((SettingFactory) ServiceLocator.SettingFactory).InsertSettingAsync(this);
        }

        public async Task DeleteSettingAsync()
        {
            await _sf.DeleteSettingAsync(Key);
            //await ((SettingFactory) ServiceLocator.SettingFactory).DeleteSettingAsync(Key);
        }

        public async Task UpdateSettingAsync(string newvalue)
        {
            await _sf.UpdateSettingAsync(Key, newvalue);
            //await ((SettingFactory) ServiceLocator.SettingFactory).UpdateSettingAsync(Key, newvalue);
        }
    }
}
