using System.Data;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class SettingPOCO : ISettingPOCO
    {
        public SettingPOCO(IDataRecord reader)
        {
            Key = reader[SqLiteDatabase.SetKey] as string;
            Value = reader[SqLiteDatabase.SetVal] as string;
        }

        public string Key { get; private set; }
        public string Value { get; private set; }
    }
}
