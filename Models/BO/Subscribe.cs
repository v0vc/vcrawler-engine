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
            // return ServiceLocator.SubscribeFactory.GetSubscribe();
            return _sf.GetSubscribe();
        }

        public async Task<List<IChannel>> GetChannelsListAsync()
        {
            return await _sf.GetChannelsListAsync();
        }

        public async Task<List<ICred>> GetCredListAsync()
        {
            return await _sf.GetCredListAsync();
        }

        public async Task<List<string>> GetChannelsIdsListDbAsync()
        {
            return await _sf.GetChannelsIdsListDbAsync();
        }
    }
}
