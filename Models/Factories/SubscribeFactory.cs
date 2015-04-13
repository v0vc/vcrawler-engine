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
    class SubscribeFactory : ISubscribeFactory
    {
        public ISubscribe GetSubscribe()
        {
            return new Subscribe();
        }

        public async Task<List<IChannel>> GetChannelsListAsync()
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var lst = new List<IChannel>();
                var fbres = await fb.GetChannelsListAsync();
                lst.AddRange(fbres.Select(p => new Channel(p)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
