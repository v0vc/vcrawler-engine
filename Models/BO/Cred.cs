using System;
using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.Factories;

namespace Models.BO
{
    public class Cred : ICred
    {
        private readonly CredFactory _credFactory;

        public Cred(ICredFactory credFactory)
        {
            _credFactory = credFactory as CredFactory;
        }

        public Cred(ICredPOCO poco, ICredFactory credFactory)
        {
            _credFactory = credFactory as CredFactory;
            Site = poco.Site;
            Login = poco.Login;
            Pass = poco.Pass;
            Cookie = poco.Cookie;
            Expired = poco.Expired;
            Autorization = poco.Autorization;
        }

        public string Site { get; set; }
        public string Login { get; set; }
        public string Pass { get; set; }
        public string Cookie { get; set; }
        public DateTime Expired { get; set; }
        public short Autorization { get; set; }

        public async Task InsertCredAsync()
        {
            // await ((CredFactory) ServiceLocator.CredFactory).InsertCRedAsync(this);
            await _credFactory.InsertCredAsync(this);
        }

        public async Task DeleteCredAsync()
        {
            await _credFactory.DeleteCredAsync(Site);
        }

        public async Task UpdateLoginAsync(string newlogin)
        {
            await _credFactory.UpdateLoginAsync(Site, newlogin);
        }

        public async Task UpdatePasswordAsync(string newpassword)
        {
            await _credFactory.UpdatePasswordAsync(Site, newpassword);
        }

        public async Task UpdateAutorizationAsync(short autorize)
        {
            await _credFactory.UpdateAutorizationAsync(Site, autorize);
        }

        public async Task UpdateCookieAsync(string newcookie)
        {
            await _credFactory.UpdateCookieAsync(Site, newcookie);
        }

        public async Task UpdateExpiredAsync(DateTime newexpired)
        {
            await _credFactory.UpdateExpiredAsync(Site, newexpired);
        }
    }
}
