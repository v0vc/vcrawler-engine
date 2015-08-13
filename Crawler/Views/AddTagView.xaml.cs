using System.Windows;
using Crawler.ViewModels;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for AddTagView.xaml
    /// </summary>
    public partial class AddTagView : Window
    {
        public AddTagView()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var atw = DataContext as AddTagViewModel;
            if (atw == null)
            {
                return;
            }
            if (!atw.ParentChannel.ChannelTags.Contains(atw.SelectedTag))
            {
                atw.ParentChannel.ChannelTags.Add(atw.SelectedTag);
            }
            Close();
        }
    }
}
