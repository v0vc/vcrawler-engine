// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Threading.Tasks;
using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.Factories
{
    public interface ISettingFactory
    {
        #region Methods

        ISetting CreateSetting();

        ISetting CreateSetting(ISettingPOCO poco);

        Task<ISetting> GetSettingDbAsync(string key);

        #endregion
    }
}
