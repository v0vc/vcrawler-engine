// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Windows;
using System.Windows.Input;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        #region Constructors

        public SettingsView()
        {
            InitializeComponent();
            KeyDown += SettingsViewKeyDown;
        }

        #endregion

        #region Event Handling

        private void SettingsViewKeyDown(object sender, KeyEventArgs e)
        {
            KeyDown -= SettingsViewKeyDown;
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        #endregion
    }
}
