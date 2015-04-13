using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Models;

namespace Interfaces.Factories
{
    public interface ICredFactory
    {
        ICred CreateCred();

        Task<ICred> GetCredDbAsync(string site);
    }
}
