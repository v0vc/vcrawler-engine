// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;
using Crawler.ViewModels;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for AddTagView.xaml
    /// </summary>
    public partial class AddTagView : Window
    {
        #region Constructors

        public AddTagView()
        {
            InitializeComponent();
            KeyDown += AddTagKeyDown;
        }

        #endregion

        #region Event Handling

        private void AddTagKeyDown(object sender, KeyEventArgs e)
        {
            KeyDown -= AddTagKeyDown;
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

        #endregion
    }
}
