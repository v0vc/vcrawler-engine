using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        private EditTagsViewModel _viewModel;

        private async void EditTagsView_OnLoaded(object sender, RoutedEventArgs e)
        {
            var etv = DataContext as EditTagsViewModel;
            if (etv == null)
            {
                return;
            }

            _viewModel = etv;

            if (etv.ParentChannel.ChannelTags.Any())
            {
                etv.ParentChannel.ChannelTags.Clear();
            }

            var lst = await etv.ParentChannel.GetChannelTagsAsync();
            foreach (ITag tag in lst)
            {
                etv.ParentChannel.ChannelTags.Add(tag);
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

        private void ButtonDeleteTag_OnClick(object sender, RoutedEventArgs e)
        {
            var tag = ((Button)e.Source).DataContext as ITag;
            if (tag != null)
            {
                var lst = DataGridTags.ItemsSource as ObservableCollection<ITag>;
                if (lst != null)
                {
                    lst.Remove(tag);
                    if (_viewModel != null)
                    {
                        _viewModel.ParentChannel.DeleteChannelTagAsync(tag.Title);
                    }
                }
            }
        }

        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
