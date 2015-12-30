// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Crawler.Common;
using Crawler.Views;
using Interfaces.Models;
using Models.Factories;

namespace Crawler.ViewModels
{
    public class EditTagsViewModel
    {
        #region Static and Readonly Fields

        private readonly Action<string> onTagDelete;
        private readonly Action<IChannel> onTagsSave;
        private readonly ObservableCollection<ITag> supportedTags;

        #endregion

        #region Fields

        private RelayCommand addCommand;
        private RelayCommand deleteTagCommand;
        private RelayCommand fillTagsCommand;
        private RelayCommand saveCommand;

        #endregion

        #region Constructors

        public EditTagsViewModel(IChannel channel,
            ObservableCollection<ITag> supportedTags,
            Action<string> onTagDelete,
            Action<IChannel> onTagsSave)
        {
            this.supportedTags = supportedTags;
            this.onTagDelete = onTagDelete;
            this.onTagsSave = onTagsSave;
            Channel = channel;
        }

        #endregion

        #region Properties

        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ?? (addCommand = new RelayCommand(Add));
            }
        }

        public IChannel Channel { get; set; }

        public RelayCommand DeleteTagCommand
        {
            get
            {
                return deleteTagCommand ?? (deleteTagCommand = new RelayCommand(DeleteTag));
            }
        }

        public RelayCommand FillTagsCommand
        {
            get
            {
                return fillTagsCommand ?? (fillTagsCommand = new RelayCommand(x => FillTags()));
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ?? (saveCommand = new RelayCommand(Save));
            }
        }

        #endregion

        #region Methods

        private void Add(object obj)
        {
            var window = obj as Window;
            if (window == null)
            {
                return;
            }
            var atvm = new AddTagViewModel(false, Channel, supportedTags);
            var atv = new AddTagView { DataContext = atvm, Owner = window, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            atv.ShowDialog();
        }

        private async void DeleteTag(object obj)
        {
            var tag = obj as ITag;
            if (tag == null)
            {
                return;
            }

            Channel.ChannelTags.Remove(tag);

            await CommonFactory.CreateSqLiteDatabase().DeleteChannelTagsAsync(Channel.ID, tag.Title);

            if (onTagDelete != null)
            {
                onTagDelete.Invoke(tag.Title);
            }
        }

        private async void FillTags()
        {
            if (Channel.ChannelTags.Any())
            {
                Channel.ChannelTags.Clear();
            }

            IEnumerable<ITag> lst = await ChannelFactory.GetChannelTagsAsync(Channel.ID);
            foreach (ITag tag in lst)
            {
                Channel.ChannelTags.Add(tag);
            }
        }

        private void Save(object obj)
        {
            var window = obj as Window;
            if (window == null)
            {
                return;
            }

            if (onTagsSave != null)
            {
                onTagsSave.Invoke(Channel);
            }

            window.Close();
        }

        #endregion
    }
}
