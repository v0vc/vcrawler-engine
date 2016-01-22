// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using Interfaces.Enums;

namespace Interfaces.Models
{
    public interface ICred
    {
        #region Properties

        short Autorization { get; set; }
        string Cookie { get; set; }
        DateTime Expired { get; set; }
        string Login { get; set; }
        string Pass { get; set; }
        SiteType Site { get; set; }
        string SiteAdress { get; }

        #endregion
    }
}
