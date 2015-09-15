// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Threading.Tasks;
using Interfaces.Models;
using Interfaces.POCO;

namespace Interfaces.Factories
{
    public interface ICredFactory
    {
        #region Methods

        ICred CreateCred();

        ICred CreateCred(ICredPOCO poco);

        Task<ICred> GetCredDbAsync(string site);

        #endregion
    }
}
