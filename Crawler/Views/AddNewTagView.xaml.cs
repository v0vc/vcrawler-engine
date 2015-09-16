// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;
using Crawler.ViewModels;
using Interfaces.Factories;
using Interfaces.Models;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for AddNewTagView.xaml
    /// </summary>
    public partial class AddNewTagView : Window
    {
        #region Constructors

        public AddNewTagView()
        {
            InitializeComponent();
            KeyDown += AddNewTag_KeyDown;
        }

        #endregion

        #region Event Handling

        private void AddNewTag_KeyDown(object sender, KeyEventArgs e)
        {
            KeyDown -= AddNewTag_KeyDown;
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

        private void AddNewTagView_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBoxTag.Focus();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var mv = DataContext as MainWindowViewModel;
            if (mv == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(mv.Model.NewTag))
            {
                return;
            }
            ITagFactory tf = mv.Model.BaseFactory.CreateTagFactory();
            ITag tag = tf.CreateTag();
            tag.Title = mv.Model.NewTag;
            if (mv.Model.Tags.Select(x => x.Title).Contains(tag.Title))
            {
                return;
            }
            mv.Model.Tags.Add(tag);
            mv.Model.NewTag = string.Empty;
            Close();
        }

        #endregion
    }
}
