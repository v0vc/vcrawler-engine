using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseAPI.POCO;
using Interfaces;
using Interfaces.Models;
using Interfaces.POCO;
using Models.Factories;

namespace Models.BO
{
    public class Cred :ICred
    {
        public string Site { get; set; }
        public string Login { get; set; }
        public string Pass { get; set; }
        public string Cookie { get; set; }
        public string Passkey { get; set; }
        public short Autorization { get; set; }

        public Cred()
        {
            
        }
        public Cred(ICredPOCO poco)
        {
            Site = poco.Site;
            Login = poco.Login;
            Pass = poco.Pass;
            Cookie = poco.Cookie;
            Passkey = poco.Passkey;
            Autorization = poco.Autorization;
        }

        public async Task InsertCredAsync()
        {
            await ((CredFactory) ServiceLocator.CredFactory).InsertCRedAsync(this);
        }

        public async Task DeleteCredAsync()
        {
            await ((CredFactory) ServiceLocator.CredFactory).DeleteCredAsync(Site);
        }

        public async Task UpdateLoginAsync(string newlogin)
        {
            await ((CredFactory) ServiceLocator.CredFactory).UpdateLoginAsync(Site, newlogin);
        }

        public async Task UpdatePasswordAsync(string newpassword)
        {
            await ((CredFactory)ServiceLocator.CredFactory).UpdatePasswordAsync(Site, newpassword);
        }

        public async Task UpdateAutorizationAsync(short autorize)
        {
            await ((CredFactory)ServiceLocator.CredFactory).UpdateAutorizationAsync(Site, autorize);
        }
    }
}
