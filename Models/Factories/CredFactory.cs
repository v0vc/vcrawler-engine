using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Factories;
using Interfaces.Models;
using Models.BO;

namespace Models.Factories
{
    class CredFactory :ICredFactory
    {
        public ICred CreateCred()
        {
            return new Cred();
        }

        public async Task InsertCRedAsync(ICred cred)
        {
            var fb = ServiceLocator.SqLiteDatabase;
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
            var fb = ServiceLocator.SqLiteDatabase;
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
            var fb = ServiceLocator.SqLiteDatabase;
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
            var fb = ServiceLocator.SqLiteDatabase;
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
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.UpdateAutorizationAsync(site, autorize);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ICred> GetCredDbAsync(string site)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var fbres = await fb.GetCredAsync(site);
                return new Cred(fbres);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
