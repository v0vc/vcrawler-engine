using System;
using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Models.BO;

namespace Models.Factories
{
    public class CredFactory : ICredFactory
    {
        private readonly ICommonFactory _c;

        public CredFactory(ICommonFactory c)
        {
            _c = c;
        }

        public ICred CreateCred()
        {
            return new Cred(this);
        }

        public async Task<ICred> GetCredDbAsync(string site)
        {
            var fb = _c.CreateSqLiteDatabase();
            // var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var fbres = await fb.GetCredAsync(site);
                return new Cred(fbres, _c.CreateCredFactory());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertCredAsync(ICred cred)
        {
            var fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.InsertCredAsync(cred);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteCredAsync(string site)
        {
            var fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.DeleteCredAsync(site);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateLoginAsync(string site, string newlogin)
        {
            var fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.UpdateLoginAsync(site, newlogin);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdatePasswordAsync(string site, string newpassword)
        {
            var fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.UpdatePasswordAsync(site, newpassword);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateAutorizationAsync(string site, short autorize)
        {
            var fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.UpdateAutorizationAsync(site, autorize);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateCookieAsync(string site, string newcookie)
        {
            var fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.UpdateCookieAsync(site, newcookie);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateExpiredAsync(string site, DateTime newexpired)
        {
            var fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.UpdateExpiredAsync(site, newexpired);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
