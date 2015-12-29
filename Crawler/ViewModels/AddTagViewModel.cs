// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Crawler.Common;
using Interfaces.Models;
using Models.BO;

namespace Crawler.ViewModels
{
    public sealed class AddTagViewModel
    {
        #region Static and Readonly Fields

        private readonly IChannel channel;

        #endregion

        #region Fields

        private RelayCommand saveCommand;

        #endregion

        #region Constructors

        public AddTagViewModel(bool isAddNewTag, IChannel channel = null, ObservableCollection<ITag> tags = null)
        {
            this.channel = channel;
            Tags = tags;
            IsAddNewTag = isAddNewTag;

            if (!IsAddNewTag && (Tags != null && Tags.Any()))
            {
                SelectedTag = Tags.First();
            }
            else
            {
                SelectedTag = new Tag();
            }
        }

        #endregion

        #region Properties

        public bool IsAddNewTag { get; set; }

        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ?? (saveCommand = new RelayCommand(Save));
            }
        }

        public ITag SelectedTag { get; set; }

        public ObservableCollection<ITag> Tags { get; set; }

        #endregion

        #region Methods

        private async void Save(object obj)
        {
            var window = obj as Window;
            if (window == null)
            {
                return;
            }
            if (!IsAddNewTag)
            {
                if (!channel.ChannelTags.Select(x => x.Title).Contains(SelectedTag.Title))
                {
                    channel.ChannelTags.Add(SelectedTag);
                }
            }
            else
            {
                SelectedTag.Title = SelectedTag.Title.Trim();
                if (string.IsNullOrEmpty(SelectedTag.Title))
                {
                    return;
                }
                if (Tags.Select(x => x.Title).Contains(SelectedTag.Title))
                {
                    return;
                }
                Tags.Add(SelectedTag);
                await SelectedTag.InsertTagAsync();
            }

            window.Close();
        }

        #endregion
    }
}
