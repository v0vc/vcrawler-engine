using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.POCO
{
    public interface ISettingPOCO
    {
        string Key { get; set; }

        string Value { get; set; }
    }
}
