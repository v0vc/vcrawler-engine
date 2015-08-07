using System.Collections.ObjectModel;
using Crawler.Common;
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class EditTagsViewModel
    {
        //public EditTagsViewModel()
        //{
        //    DeleteCommand = new RelayCommand(x => Delete());
        //}

        public ITag SelectedTag { get; set; }
        public RelayCommand DeleteCommand { get; set; }
        public IChannel ParentChannel { get; set; }
        public ObservableCollection<ITag> Tags { get; set; }

        //private void Delete()
        //{
        //    Tags.Remove(SelectedTag);
        //}
    }
}
