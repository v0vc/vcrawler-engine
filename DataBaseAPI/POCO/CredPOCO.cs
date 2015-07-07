using System;
using System.Data;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class CredPOCO : ICredPOCO
    {
        public CredPOCO(IDataRecord reader)
        {
            Site = reader[SqLiteDatabase.CredSite] as string;
            Login = reader[SqLiteDatabase.CredLogin] as string;
            Pass = reader[SqLiteDatabase.CredPass] as string;
            Cookie = reader[SqLiteDatabase.CredCookie] as string;
            Expired = (DateTime)reader[SqLiteDatabase.CredExpired];
            Autorization = Convert.ToInt16(reader[SqLiteDatabase.CredAutorization]);
        }

        public string Site { get; set; }
        public string Login { get; set; }
        public string Pass { get; set; }
        public string Cookie { get; set; }
        public DateTime Expired { get; set; }
        public short Autorization { get; set; }
    }
}
