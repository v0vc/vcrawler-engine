using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Models;

namespace Interfaces.Factories
{
    public interface ISettingFactory
    {
        ISetting CreateSetting();

        Task<ISetting> GetSettingDbAsync(string key);
    }
}
