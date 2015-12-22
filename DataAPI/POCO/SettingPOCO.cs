// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved


namespace DataAPI.POCO
{
    public class SettingPOCO
    {
        #region Constructors

        public SettingPOCO(string key, string value)
        {
            Key = key;
            Value = value;
        }

        #endregion

        #region ISettingPOCO Members

        public string Key { get; private set; }
        public string Value { get; private set; }

        #endregion
    }
}
