using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Models.BO;

namespace Models.Factories
{
    public class SubscribeFactory : ISubscribeFactory
    {
        private readonly ICommonFactory _c;

        public SubscribeFactory(ICommonFactory c)
        {
            _c = c;
        }

        public ISubscribe GetSubscribe()
        {
            return new Subscribe(this);
        }

        public async Task<List<IChannel>> GetChannelsListAsync()
        {
            var fb = _c.CreateSqLiteDatabase();

            // var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var lst = new List<IChannel>();
                var fbres = await fb.GetChannelsListAsync();
                lst.AddRange(fbres.Select(p => new Channel(p, _c.CreateChannelFactory())));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ICred>> GetCredListAsync()
        {
            var fb = _c.CreateSqLiteDatabase();
            try
            {
                var lst = new List<ICred>();
                var fbres = await fb.GetCredListAsync();
                lst.AddRange(fbres.Select(p => new Cred(p, _c.CreateCredFactory())));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<string>> GetChannelsIdsListDbAsync()
        {
            var fb = _c.CreateSqLiteDatabase();
            try
            {
                return await fb.GetChannelsIdsListDbAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
