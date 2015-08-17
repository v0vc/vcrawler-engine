using System.Windows;
using System.Windows.Input;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for FfmpegView.xaml
    /// </summary>
    public partial class FfmpegView : Window
    {
        public FfmpegView()
        {
            InitializeComponent();
            KeyDown += FfmpegView_KeyDown;
        }

        private void FfmpegView_KeyDown(object sender, KeyEventArgs e)
        {
            KeyDown -= FfmpegView_KeyDown;
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
