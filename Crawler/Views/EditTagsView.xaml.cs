// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using Crawler.ViewModels;
using Interfaces.Models;

namespace Crawler.Views
{
    /// <summary>
    ///     Interaction logic for EditTagsView.xaml
    /// </summary>
    public partial class EditTagsView : Window
    {
        #region Constructors

        public EditTagsView()
        {
            InitializeComponent();
            KeyDown += EditTagsViewKeyDown;
        }

        #endregion

        #region Properties

        public EditTagsViewModel ViewModel
        {
            get
            {
                return DataContext as EditTagsViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        #endregion

        #region Event Handling

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            var atvm = new AddTagViewModel { ParentChannel = ViewModel.ParentChannel };
            foreach (ITag tag in ViewModel.Tags)
            {
                atvm.Tags.Add(tag);
            }

            atvm.SelectedTag = atvm.Tags.First();

            var atv = new AddTagView { DataContext = atvm, Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };

            atv.ShowDialog();
        }

        private void ButtonDeleteTag_OnClick(object sender, RoutedEventArgs e)
        {
            var tag = ((Button)e.Source).DataContext as ITag;
            if (tag == null)
            {
                return;
            }

            var lst = DataGridTags.ItemsSource as ObservableCollection<ITag>;
            if (lst == null)
            {
                return;
            }
            lst.Remove(tag);

            if (ViewModel == null)
            {
                return;
            }

            ViewModel.ParentChannel.DeleteChannelTagAsync(tag.Title);
            if (!ViewModel.Channels.Any(x => x.ChannelTags.Select(y => y.Title).Contains(tag.Title)))
            {
                ITag ctag = ViewModel.CurrentTags.FirstOrDefault(x => x.Title == tag.Title);
                if (ctag != null)
                {
                    ViewModel.CurrentTags.Remove(ctag);
                }
            }
        }

        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void EditTagsViewKeyDown(object sender, KeyEventArgs e)
        {
            KeyDown -= EditTagsViewKeyDown;
            if (e.Key == Key.Escape)
            {
                Close();
            }
            if (e.Key == Key.Enter)
            {
                // нажмем кнопку программно
                var peer = new ButtonAutomationPeer(ButtonSave);
                var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                if (invokeProv != null)
                {
                    invokeProv.Invoke();
                }
            }
        }

        private async void EditTagsView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ParentChannel.ChannelTags.Any())
            {
                ViewModel.ParentChannel.ChannelTags.Clear();
            }

            IEnumerable<ITag> lst = await ViewModel.ParentChannel.GetChannelTagsAsync();
            foreach (ITag tag in lst)
            {
                ViewModel.ParentChannel.ChannelTags.Add(tag);
            }
        }

        #endregion
    }
}
