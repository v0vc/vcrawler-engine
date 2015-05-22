using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;

using Crawler.ViewModels;

namespace Crawler.Views
{
    /// <summary>
    /// Interaction logic for AddChanelView.xaml
    /// </summary>
    public partial class AddChanelView : Window
    {
        public AddChanelView()
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
            if (e.Key == Key.Enter)
            {
                //нажмем кнопку программно
                var peer = new ButtonAutomationPeer(ButtonOk);
                var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                if (invokeProv != null)
                    invokeProv.Invoke();
            }
        }

        private void AddChanelView_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBoxLink.Focus();
            var text = Clipboard.GetData(DataFormats.Text) as string;
            if (string.IsNullOrWhiteSpace(text) || text.Contains(Environment.NewLine))
                return;
            var viewModel = DataContext as MainWindowViewModel;
            if (viewModel != null)
                viewModel.Model.NewChannelName = text;
            TextBoxLink.SelectAll();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
