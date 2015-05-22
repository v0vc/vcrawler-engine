using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Crawler.ViewModels;

namespace Crawler.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        public MainWindowViewModel ViewModel
        {
            get { return DataContext as MainWindowViewModel; }
            set { DataContext = value; }
        }

        public SettingsView()
        {
            InitializeComponent();
            KeyDown += SettingsView_KeyDown;

        }

        private void SettingsView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                KeyDown -= SettingsView_KeyDown;
                Close();
            }
        }

        private async void SettingsView_OnLoaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.Model.LoadSettings();
        }
    }
}
