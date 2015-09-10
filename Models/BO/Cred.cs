// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Threading.Tasks;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO
{
    public class Cred : ICred
    {
        #region Static and Readonly Fields

        private readonly CredFactory _credFactory;

        #endregion

        #region Constructors

        public Cred(CredFactory credFactory)
        {
            _credFactory = credFactory;
        }

        private Cred()
        {
        }

        #endregion

        #region ICred Members

        public short Autorization { get; set; }
        public string Cookie { get; set; }
        public DateTime Expired { get; set; }
        public string Login { get; set; }
        public string Pass { get; set; }
        public string Site { get; set; }

        public async Task DeleteCredAsync()
        {
            await _credFactory.DeleteCredAsync(Site);
        }

        public async Task InsertCredAsync()
        {
            // await ((CredFactory) ServiceLocator.CredFactory).InsertCRedAsync(this);
            await _credFactory.InsertCredAsync(this);
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

        public async Task UpdateLoginAsync(string newlogin)
        {
            await _credFactory.UpdateLoginAsync(Site, newlogin);
        }

        public async Task UpdatePasswordAsync(string newpassword)
        {
            await _credFactory.UpdatePasswordAsync(Site, newpassword);
        }

        #endregion
    }
}
