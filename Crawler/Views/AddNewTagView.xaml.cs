using System.Linq;
using System.Windows;
using Crawler.ViewModels;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for AddNewTagView.xaml
    /// </summary>
    public partial class AddNewTagView : Window
    {
        public AddNewTagView()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainWindowViewModel;
            if (mv != null)
            {
                if (!string.IsNullOrEmpty(mv.Model.NewTag))
                {
                    var tf = mv.Model.BaseFactory.CreateTagFactory();
                    var tag = tf.CreateTag();
                    tag.Title = mv.Model.NewTag;
                    if (!mv.Model.Tags.Select(x => x.Title).Contains(tag.Title))
                    {
                        mv.Model.Tags.Add(tag);
                        Close();
                    }
                }
            }
        }

        private void AddNewTagView_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBoxTag.Focus();
        }
    }
}
