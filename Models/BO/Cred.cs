// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using Extensions.Helpers;
using Interfaces.Enums;
using Interfaces.Models;

namespace Models.BO
{
    public class Cred : ICred
    {
        #region ICred Members

        public short Autorization { get; set; }
        public string Cookie { get; set; }
        public DateTime Expired { get; set; }
        public string Login { get; set; }
        public string Pass { get; set; }
        public SiteType Site { get; set; }

        public string SiteAdress
        {
            get
            {
                return EnumHelper.GetAttributeOfType(Site);
            } 
        }

        #endregion
    }
}
