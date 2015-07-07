using System;

namespace Interfaces.POCO
{
    public interface ICredPOCO
    {
        string Site { get; set; }
        string Login { get; set; }
        string Pass { get; set; }
        string Cookie { get; set; }
        DateTime Expired { get; set; }
        short Autorization { get; set; }
    }
}
