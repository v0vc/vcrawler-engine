// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

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
        #region Constructors

        public AddLinkView()
        {
            InitializeComponent();
            KeyDown += AddLinkViewKeyDown;
        }

        #endregion

        #region Event Handling

        private void AddLinkViewKeyDown(object sender, KeyEventArgs e)
        {
            KeyDown -= AddLinkViewKeyDown;
            if (e.Key == Key.Escape)
            {
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

        private void AddLink_OnAudioMouseEnter(object sender, MouseButtonEventArgs e)
        {
            checkBoxAudio.IsChecked = !checkBoxAudio.IsChecked;
        }

        private void AddLink_OnHdMouseEnter(object sender, MouseButtonEventArgs e)
        {
            checkBoxHd.IsChecked = !checkBoxHd.IsChecked;
        }

        private void ButtonGo_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}
