using System;

namespace Interfaces.POCO
{
    public interface ICredPOCO
    {
        string Site { get; set; }

        string Login { get; set; }

        string Pass { get; set; }

        string Cookie { get; set; }

        string Passkey { get; set; }

        Int16 Autorization { get; set; }
    }
}
