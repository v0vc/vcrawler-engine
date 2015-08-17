using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.BO;

namespace Models.Factories
{
    public class TagFactory : ITagFactory
    {
        private readonly ICommonFactory _c;

        public TagFactory(ICommonFactory c)
        {
            _c = c;
        }

        public ITag CreateTag()
        {
            return new Tag(this);
        }

        public ITag CreateTag(ITagPOCO poco)
        {
            var tag = new Tag(this) { Title = poco.Title };
            return tag;
        }

        public async Task DeleteTagAsync(string tag)
        {
            var fb = _c.CreateSqLiteDatabase();

            // var fb = ServiceLocator.SqLiteDatabase;
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
            var fb = _c.CreateSqLiteDatabase();
            try
            {
                await fb.InsertTagAsync(tag);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<IChannel>> GetChannelsByTagAsync(string tag)
        {
            var fb = _c.CreateSqLiteDatabase();
            var cf = _c.CreateChannelFactory();
            try
            {
                var lst = new List<IChannel>();
                var fbres = await fb.GetChannelsByTagAsync(tag);
                lst.AddRange(fbres.Select(poco => cf.CreateChannel(poco)));
                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
