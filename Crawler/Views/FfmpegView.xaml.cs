﻿// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Windows;
using System.Windows.Input;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for FfmpegView.xaml
    /// </summary>
    public partial class FfmpegView : Window
    {
        #region Constructors

        public FfmpegView()
        {
            InitializeComponent();
            KeyDown += FfmpegViewKeyDown;
        }

        #endregion

        #region Event Handling

        private void FfmpegViewKeyDown(object sender, KeyEventArgs e)
        {
            KeyDown -= FfmpegViewKeyDown;
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        #endregion
    }
}
