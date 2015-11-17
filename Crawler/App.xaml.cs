// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Windows;
using Crawler.ViewModels;
using Crawler.Views;

namespace Crawler
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Event Handling

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var mv = new MainWindow { DataContext = new MainWindowViewModel() };
            mv.Show();
        }

        #endregion
    }
}
