// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Data;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class CredPOCO : ICredPOCO
    {
        #region Constructors

        public CredPOCO(IDataRecord reader)
        {
            Site = reader[SqLiteDatabase.CredSite] as string;
            Login = reader[SqLiteDatabase.CredLogin] as string;
            Pass = reader[SqLiteDatabase.CredPass] as string;
            Cookie = reader[SqLiteDatabase.CredCookie] as string;
            Expired = (DateTime)reader[SqLiteDatabase.CredExpired];
            Autorization = Convert.ToInt16(reader[SqLiteDatabase.CredAutorization]);
        }

        #endregion

        #region ICredPOCO Members

        public short Autorization { get; private set; }
        public string Cookie { get; private set; }
        public DateTime Expired { get; private set; }
        public string Login { get; private set; }
        public string Pass { get; private set; }
        public string Site { get; private set; }

        #endregion
    }
}
