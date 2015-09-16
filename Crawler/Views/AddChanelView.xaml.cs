// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;
using Crawler.ViewModels;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for AddChanelView.xaml
    /// </summary>
    public partial class AddChanelView : Window
    {
        #region Constructors

        public AddChanelView()
        {
            InitializeComponent();
            KeyDown += AddChanelViewKeyDown;
        }

        #endregion

        #region Event Handling

        private void AddChanelViewKeyDown(object sender, KeyEventArgs e)
        {
            KeyDown -= AddChanelViewKeyDown;
            if (e.Key == Key.Escape)
            {
                Close();
            }
            if (e.Key == Key.Enter)
            {
                // нажмем кнопку программно
                var peer = new ButtonAutomationPeer(ButtonOk);
                var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                if (invokeProv != null)
                {
                    invokeProv.Invoke();
                }
            }
        }

        private void AddChanelView_OnLoaded(object sender, RoutedEventArgs e)
        {
            var context = DataContext as MainWindowViewModel;
            if (context == null)
            {
                return;
            }

            if (context.Model.IsEditMode)
            {
                TextBoxLink.Text = context.Model.SelectedChannel.ID;
                TextBoxLink.IsEnabled = true;
                TextBoxLink.IsReadOnly = true;

                TextBoxName.Text = context.Model.SelectedChannel.Title;
                TextBoxName.Focus();
                TextBoxName.SelectAll();
                ComboBoxSities.IsEnabled = false;
            }
            else
            {
                TextBoxLink.Focus();
                var text = Clipboard.GetData(DataFormats.Text) as string;
                if (string.IsNullOrWhiteSpace(text) || text.Contains(Environment.NewLine))
                {
                    return;
                }
                context.Model.NewChannelLink = text;
                context.Model.NewChannelTitle = string.Empty;
                TextBoxLink.SelectAll();
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}
