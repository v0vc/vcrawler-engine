// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.ObjectModel;
using System.Windows;
using Crawler.Common;
using Crawler.Views;
using DataAPI.Database;
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class EditTagsViewModel
    {
        #region Static and Readonly Fields

        private readonly SqLiteDatabase db;
        private readonly Action<string> onTagDelete;
        private readonly Action<IChannel> onTagsSave;
        private readonly ObservableCollection<ITag> supportedTags;

        #endregion

        #region Fields

        private RelayCommand addCommand;
        private RelayCommand deleteTagCommand;
        private RelayCommand saveCommand;

        #endregion

        #region Constructors

        public EditTagsViewModel(IChannel channel,
            ObservableCollection<ITag> supportedTags,
            SqLiteDatabase db,
            Action<string> onTagDelete,
            Action<IChannel> onTagsSave)
        {
            this.supportedTags = supportedTags;
            this.db = db;
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

            await db.DeleteChannelTagsAsync(Channel.ID, tag.Title).ConfigureAwait(false);

            if (onTagDelete != null)
            {
                onTagDelete.Invoke(tag.Title);
            }
        }

        private async void Save(object obj)
        {
            var window = obj as Window;
            if (window == null)
            {
                return;
            }

            await db.InsertChannelTagsAsync(Channel.ID, Channel.ChannelTags).ConfigureAwait(false);

            if (onTagsSave != null)
            {
                onTagsSave.Invoke(Channel);
            }

            window.Close();
        }

        #endregion
    }
}
