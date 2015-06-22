using System;
using System.Net;
using System.Threading.Tasks;

namespace Interfaces.Models
{
    public interface ICred
    {
        string Site { get; set; }

        string Login { get; set; }

        string Pass { get; set; }

        string Cookie { get; set; }

        string Passkey { get; set; }

        Int16 Autorization { get; set; }

        Task InsertCredAsync();

        Task DeleteCredAsync();

        Task UpdateLoginAsync(string newlogin);

        Task UpdatePasswordAsync(string newpassword);

        Task UpdateAutorizationAsync(short autorize);

        
    }
}
