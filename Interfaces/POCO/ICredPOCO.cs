using System;

namespace Interfaces.POCO
{
    public interface ICredPOCO
    {
        string Site { get; }
        string Login { get; }
        string Pass { get; }
        string Cookie { get; }
        DateTime Expired { get; }
        short Autorization { get; }
    }
}
