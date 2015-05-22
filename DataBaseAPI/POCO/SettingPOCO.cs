using System.Data;
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
