// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.ComponentModel;

namespace Interfaces.Enums
{
    public enum PrivacyStatus : byte
    {
        [Description("public")]
        Public = 0,
        [Description("unlisted")]
        Unlisted = 1,
        [Description("private")]
        Private = 2
    }
}
