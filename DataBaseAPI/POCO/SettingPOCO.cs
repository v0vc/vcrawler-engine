// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Data;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class SettingPOCO : ISettingPOCO
    {
        #region Constructors

        public SettingPOCO(IDataRecord reader)
        {
            Key = reader[SqLiteDatabase.SetKey] as string;
            Value = reader[SqLiteDatabase.SetVal] as string;
        }

        #endregion

        #region ISettingPOCO Members

        public string Key { get; private set; }
        public string Value { get; private set; }

        #endregion
    }
}
