using System.Collections.Generic;
using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO
{
    public class Subscribe : ISubscribe
    {
        private readonly SubscribeFactory _sf;

        public Subscribe(ISubscribeFactory sf)
        {
            _sf = sf as SubscribeFactory;
        }

        public ISubscribe GetSubscribe()
        {
            return _sf.GetSubscribe();
            //return ServiceLocator.SubscribeFactory.GetSubscribe();
        }

        public async Task<List<IChannel>> GetChannelsListAsync()
        {
            return await _sf.GetChannelsListAsync();
            //return await ((SubscribeFactory)ServiceLocator.SubscribeFactory).GetChannelsListAsync();
        }

        public async Task<List<ICred>> GetCredListAsync()
        {
            return await _sf.GetCredListAsync();
            //return await ((SubscribeFactory)ServiceLocator.SubscribeFactory).GetCredListAsync();
        }

        public async Task<List<string>> GetChannelsIdsListDbAsync()
        {
            return await _sf.GetChannelsIdsListDbAsync();
        }
    }
}
