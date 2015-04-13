using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO
{
    public class Subscribe : ISubscribe
    {
        public ISubscribe GetSubscribe()
        {
            return ServiceLocator.SubscribeFactory.GetSubscribe();
        }

        async Task<List<IChannel>> ISubscribe.GetChannelsListAsync()
        {
            return await ((SubscribeFactory)ServiceLocator.SubscribeFactory).GetChannelsListAsync();
        }
    }
}
