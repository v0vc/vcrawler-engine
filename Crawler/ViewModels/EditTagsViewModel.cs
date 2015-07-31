using System.Collections.ObjectModel;
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class EditTagsViewModel
    {
        public IChannel ParentChannel { get; set; }
        public ObservableCollection<ITag> Tags { get; set; }
    }
}