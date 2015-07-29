using System.Collections.Generic;
using System.Threading.Tasks;
using Interfaces.Models;
using Models.Factories;

namespace Models.BO
{
    public class Tag : ITag
    {
        private readonly TagFactory _tf;

        private Tag()
        {
        }

        public Tag(TagFactory tf)
        {
            _tf = tf;
        }

        public string Title { get; set; }

        public async Task DeleteTagAsync()
        {
            // return ServiceLocator.TagFactory.DeleteTagAsync(Title);
            await _tf.DeleteTagAsync(Title);
        }

        public async Task InsertTagAsync()
        {
            await _tf.InsertTagAsync(this);
        }

        public async Task<List<IChannel>> GetChannelsByTagAsync()
        {
            return await _tf.GetChannelsByTagAsync(Title);
        }
    }
}
