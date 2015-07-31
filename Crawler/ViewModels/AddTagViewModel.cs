using System.Collections.ObjectModel;
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class AddTagViewModel
    {
        public AddTagViewModel()
        {
            Tags = new ObservableCollection<ITag>();
        }

        public IChannel ParentChannel { get; set; }
        public ITag SelectedTag { get; set; }
        public ObservableCollection<ITag> Tags { get; set; }
    }
}
