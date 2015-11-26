// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Crawler.Common;
using Crawler.Views;
using Interfaces.Models;
using Models.BO.Channels;

namespace Crawler.ViewModels
{
    public class EditTagsViewModel
    {
        #region Fields

        private RelayCommand addCommand;
        private RelayCommand deleteTagCommand;
        private RelayCommand fillTagsCommand;
        private RelayCommand saveCommand;

        #endregion

        #region Properties

        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ?? (addCommand = new RelayCommand(Add));
            }
        }

        public ObservableCollection<IChannel> Channels { get; set; }
        public ObservableCollection<ITag> CurrentTags { get; set; }

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

        public YouChannel ParentChannel { get; set; }

        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ?? (saveCommand = new RelayCommand(Save));
            }
        }

        public ITag SelectedTag { get; set; }
        public IList<ITag> Tags { get; set; }

        #endregion

        #region Methods

        private void Add(object obj)
        {
            var window = obj as Window;
            if (window == null)
            {
                return;
            }
            var atvm = new AddTagViewModel { ParentChannel = ParentChannel };
            foreach (ITag tag in Tags)
            {
                atvm.Tags.Add(tag);
            }

            atvm.SelectedTag = atvm.Tags.First();

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

            ParentChannel.ChannelTags.Remove(tag);

            await ParentChannel.DeleteChannelTagAsync(tag.Title);
            if (!Channels.Any(x => x.ChannelTags.Select(y => y.Title).Contains(tag.Title)))
            {
                ITag ctag = CurrentTags.FirstOrDefault(x => x.Title == tag.Title);
                if (ctag != null)
                {
                    CurrentTags.Remove(ctag);
                }
            }
        }

        private async void FillTags()
        {
            if (ParentChannel.ChannelTags.Any())
            {
                ParentChannel.ChannelTags.Clear();
            }

            IEnumerable<ITag> lst = await ParentChannel.GetChannelTagsAsync();
            foreach (ITag tag in lst)
            {
                ParentChannel.ChannelTags.Add(tag);
            }
        }

        private async void Save(object obj)
        {
            var window = obj as Window;
            if (window == null)
            {
                return;
            }

            if (!CurrentTags.Any())
            {
                foreach (YouChannel channel in Channels.OfType<YouChannel>())
                {
                    IEnumerable<ITag> tags = await channel.GetChannelTagsAsync();
                    foreach (ITag tag in tags)
                    {
                        channel.ChannelTags.Add(tag);
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

            window.Close();
        }

        #endregion
    }
}
