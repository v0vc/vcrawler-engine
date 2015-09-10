// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;

namespace Interfaces.POCO
{
    public interface ICredPOCO
    {
        #region Properties

        short Autorization { get; }
        string Cookie { get; }
        DateTime Expired { get; }
        string Login { get; }
        string Pass { get; }
        string Site { get; }

        #endregion
    }
}
