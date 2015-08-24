﻿using System.Windows;
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
        public AddTagView()
        {
            InitializeComponent();
            KeyDown += AddTag_KeyDown;
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

        private void AddTag_KeyDown(object sender, KeyEventArgs e)
        {
            KeyDown -= AddTag_KeyDown;
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
    }
}