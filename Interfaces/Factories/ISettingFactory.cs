using System.Threading.Tasks;
using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.Factories
{
    public interface ISettingFactory
    {
        ISetting CreateSetting();
        ISetting CreateSetting(ISettingPOCO poco);
        Task<ISetting> GetSettingDbAsync(string key);
    }
}
