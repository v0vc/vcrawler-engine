using System.Collections.ObjectModel;
using System.Linq;
using Crawler.Common;
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class EditTagsViewModel
    {
        public EditTagsViewModel()
        {
            SaveCommand = new RelayCommand(x => Save());
        }
        public ITag SelectedTag { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public IChannel ParentChannel { get; set; }
        public ObservableCollection<ITag> CurrentTags { get; set; }
        public ObservableCollection<ITag> Tags { get; set; }

        private void Save()
        {
            foreach (ITag tag in ParentChannel.ChannelTags)
            {
                ParentChannel.InsertChannelTagAsync(tag.Title);
                if (!CurrentTags.Select(x => x.Title).Contains(tag.Title))
                {
                    CurrentTags.Add(tag);
                }
            }
        }
    }
}
