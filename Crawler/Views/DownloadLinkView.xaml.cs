﻿// This file contains my intellectual property. Release of this file requires prior approval from me.
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
    ///     Interaction logic for DownloadLinkView.xaml
    /// </summary>
    public partial class DownloadLinkView : Window
    {
        #region Constructors

        public DownloadLinkView()
        {
            InitializeComponent();
            KeyDown += AddLinkViewKeyDown;
        }

        #endregion

        #region Event Handling

        private void AddLinkViewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                KeyDown -= AddLinkViewKeyDown;
                Close();
            }
            if (e.Key == Key.Enter)
            {
                KeyDown -= AddLinkViewKeyDown;
                // нажмем кнопку программно
                var peer = new ButtonAutomationPeer(buttonGo);
                var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                if (invokeProv != null)
                {
                    invokeProv.Invoke();
                }
            }
        }

        //private void AddLinkView_OnLoaded(object sender, RoutedEventArgs e)
        //{
        //    var text = Clipboard.GetData(DataFormats.Text) as string;
        //    if (string.IsNullOrWhiteSpace(text) || text.Contains(Environment.NewLine))
        //    {
        //        return;
        //    }
        //    TextBoxLink.Text = text;
        //    TextBoxLink.Focus();
        //    TextBoxLink.SelectAll();
        //}

        private void AddLink_OnAudioMouseEnter(object sender, MouseButtonEventArgs e)
        {
            checkBoxAudio.IsChecked = !checkBoxAudio.IsChecked;
        }

        private void AddLink_OnHdMouseEnter(object sender, MouseButtonEventArgs e)
        {
            checkBoxHd.IsChecked = !checkBoxHd.IsChecked;
        }

        //private void ButtonGo_OnClick(object sender, RoutedEventArgs e)
        //{
        //    Close();
        //}

        #endregion
    }
}
