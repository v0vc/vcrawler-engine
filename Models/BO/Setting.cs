using System.Threading.Tasks;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO
{
    public class Setting : ISetting
    {
        private readonly SettingFactory _sf;

        private Setting()
        {
        }

        public Setting(SettingFactory sf)
        {
            _sf = sf;
        }

        public string Key { get; set; }
        public string Value { get; set; }

        public async Task InsertSettingAsync()
        {
            // await ((SettingFactory) ServiceLocator.SettingFactory).InsertSettingAsync(this);
            await _sf.InsertSettingAsync(this);
        }

        public async Task DeleteSettingAsync()
        {
            await _sf.DeleteSettingAsync(Key);
        }

        public async Task UpdateSettingAsync(string newvalue)
        {
            await _sf.UpdateSettingAsync(Key, newvalue);
        }
    }
}
