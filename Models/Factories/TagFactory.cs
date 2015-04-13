using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Models;
using Models.BO;

namespace Models.Factories
{
    public class TagFactory
    {
        public async Task DeleteTagAsync(string tag)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.DeleteTagAsync(tag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task InsertTagAsync(ITag tag)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                await fb.InsertTagAsync(tag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IChannel>> GetChannelsByTagAsync(string tag)
        {
            var fb = ServiceLocator.SqLiteDatabase;
            try
            {
                var lst = new List<IChannel>();
                var fbres = await fb.GetChannelsByTagAsync(tag);
                lst.AddRange(fbres.Select(item => new Channel(item)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
