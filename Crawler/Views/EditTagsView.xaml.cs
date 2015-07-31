using System.Linq;
using System.Windows;
using Crawler.ViewModels;
using Interfaces.Models;

namespace Crawler.Views
{
    /// <summary>
    /// Interaction logic for EditTagsView.xaml
    /// </summary>
    public partial class EditTagsView : Window
    {
        public EditTagsView()
        {
            InitializeComponent();
        }

        private async void EditTagsView_OnLoaded(object sender, RoutedEventArgs e)
        {
            var ch = DataContext as IChannel;
            if (ch == null)
            {
                return;
            }

            if (!ch.ChannelTags.Any())
            {
                await ch.GetChannelTagsAsync();
            }
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            var etvm = DataContext as EditTagsViewModel;
            if (etvm == null)
            {
                return;
            }

            var atvm = new AddTagViewModel { ParentChannel = etvm.ParentChannel };
            foreach (ITag tag in etvm.Tags)
            {
                atvm.Tags.Add(tag);
            }

            atvm.SelectedTag = atvm.Tags.First();

            var atv = new AddTagView
            {
                DataContext = atvm, 
                Owner = this, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            
            atv.ShowDialog();
        }
    }
}
