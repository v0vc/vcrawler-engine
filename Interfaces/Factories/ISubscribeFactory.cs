using System;
using Interfaces.Models;

namespace Interfaces.Factories
{
    public interface ISubscribeFactory
    {
        ISubscribe GetSubscribe();
    }
}
