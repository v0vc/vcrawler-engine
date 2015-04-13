using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Models
{
    public interface ISetting
    {
        string Key { get; set; }

        string Value { get; set; }

        Task InsertSettingAsync();

        Task DeleteSettingAsync();

        Task UpdateSettingAsync(string newvalue);
    }
}
