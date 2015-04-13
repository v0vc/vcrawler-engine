using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class SettingPOCO :ISettingPOCO
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public SettingPOCO(IDataRecord reader)
        {
            Key = reader[SqLiteDatabase.SetKey] as string;
            Value = reader[SqLiteDatabase.SetVal] as string;
        }
    }
}
