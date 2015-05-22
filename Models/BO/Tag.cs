using System.Collections.Generic;
using System.Threading.Tasks;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Models.Factories;

namespace Models.BO
{
    public class Tag :ITag
    {
        private readonly TagFactory _tf;
        public string Title { get; set; }
        public List<IChannel> Channels { get; set; }

        public Tag(ITagFactory tf)
        {
            _tf = tf as TagFactory;
        }
        public Tag(ITagPOCO tag, ITagFactory tf)
        {
            _tf = tf as TagFactory;
            Title = tag.Title;
        }

        public async Task DeleteTagAsync()
        {
            await _tf.DeleteTagAsync(Title);
            //return ServiceLocator.TagFactory.DeleteTagAsync(Title);
        }

        public async Task InsertTagAsync()
        {
            await _tf.InsertTagAsync(this);
            //return ServiceLocator.TagFactory.InsertTagAsync(this);
        }

        public async Task<List<IChannel>> GetChannelsByTagAsync()
        {
            return await _tf.GetChannelsByTagAsync(Title);
            //return ServiceLocator.TagFactory.GetChannelsByTagAsync(Title);
        }
    }
}
