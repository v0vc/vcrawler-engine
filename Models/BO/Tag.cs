using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Models;
using Interfaces.POCO;

namespace Models.BO
{
    public class Tag :ITag
    {
        public string Title { get; set; }
        public List<IChannel> Channels { get; set; }

        public Tag(ITagPOCO tag)
        {
            Title = tag.Title;
        }

        public Task DeleteTagAsync()
        {
            return ServiceLocator.TagFactory.DeleteTagAsync(Title);
        }

        public Task InsertTagAsync()
        {
            return ServiceLocator.TagFactory.InsertTagAsync(this);
        }

        public Task<List<IChannel>> GetChannelsByTagAsync()
        {
            return ServiceLocator.TagFactory.GetChannelsByTagAsync(Title);
        }
    }
}
