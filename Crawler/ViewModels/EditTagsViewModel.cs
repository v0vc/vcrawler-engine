// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Crawler.Common;
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class EditTagsViewModel
    {
        #region Constructors

        public EditTagsViewModel()
        {
            SaveCommand = new RelayCommand(x => Save());
        }

        #endregion

        #region Properties

        public ObservableCollection<IChannel> Channels { get; set; }
        public ObservableCollection<ITag> CurrentTags { get; set; }
        public IChannel ParentChannel { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public ITag SelectedTag { get; set; }
        public ObservableCollection<ITag> Tags { get; set; }

        #endregion

        #region Methods

        private async void Save()
        {
            if (!CurrentTags.Any())
            {
                foreach (IChannel ch in Channels)
                {
                    IEnumerable<ITag> tags = await ch.GetChannelTagsAsync();
                    foreach (ITag tag in tags)
                    {
                        ch.ChannelTags.Add(tag);
                        if (!CurrentTags.Select(x => x.Title).Contains(tag.Title))
                        {
                            CurrentTags.Add(tag);
                        }
                    }
                }
            }

            foreach (ITag tag in ParentChannel.ChannelTags)
            {
                await ParentChannel.InsertChannelTagAsync(tag.Title);

                if (!CurrentTags.Select(x => x.Title).Contains(tag.Title))
                {
                    CurrentTags.Add(tag);
                }
            }
        }

        #endregion
    }
}
