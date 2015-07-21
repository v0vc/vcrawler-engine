using System.Windows;
using System.Windows.Input;
using Interfaces.Models;
using SitesAPI;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for EditDescriptionView.xaml
    /// </summary>
    public partial class EditDescriptionView : Window
    {
        public EditDescriptionView()
        {
            InitializeComponent();
            KeyDown += AddChanelView_KeyDown;
        }

        private void AddChanelView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                KeyDown -= AddChanelView_KeyDown;
                Close();
            }
        }

        private async void EditDescriptionView_OnLoaded(object sender, RoutedEventArgs e)
        {
            var context = DataContext as IVideoItem;
            if (context == null)
            {
                return;
            }

            var id = context.ID;
            var link = string.Format("http://img.youtube.com/vi/{0}/0.jpg", id);

            context.LargeThumb = await SiteHelper.GetStreamFromUrl(link);
        }
    }
}
