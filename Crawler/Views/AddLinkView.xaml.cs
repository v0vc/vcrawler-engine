using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for AddLinkView.xaml
    /// </summary>
    public partial class AddLinkView : Window
    {
        public AddLinkView()
        {
            InitializeComponent();
            KeyDown += AddLinkView_KeyDown;
        }

        private void AddLinkView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                KeyDown -= AddLinkView_KeyDown;
                Close();
            }
            if (e.Key == Key.Enter)
            {
                // нажмем кнопку программно
                var peer = new ButtonAutomationPeer(ButtonGo);
                var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                if (invokeProv != null)
                {
                    invokeProv.Invoke();
                }
            }
        }

        private void AddLink_OnMouseEnter(object sender, MouseButtonEventArgs e)
        {
            CheckBoxHd.IsChecked = !CheckBoxHd.IsChecked;
        }

        private void AddLinkView_OnLoaded(object sender, RoutedEventArgs e)
        {
            var text = Clipboard.GetData(DataFormats.Text) as string;
            if (string.IsNullOrWhiteSpace(text) || text.Contains(Environment.NewLine))
            {
                return;
            }
            TextBoxLink.Text = text;
            TextBoxLink.Focus();
            TextBoxLink.SelectAll();
        }

        private void ButtonGo_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
