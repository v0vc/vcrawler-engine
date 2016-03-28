// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Windows;
using System.Windows.Input;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : Window
    {
        #region Constructors

        public AboutView()
        {
            InitializeComponent();
            KeyDown += ViewKeyDown;
        }

        #endregion

        #region Event Handling

        private void ViewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
            {
                return;
            }
            KeyDown -= ViewKeyDown;
            Close();
        }

        #endregion
    }
}
