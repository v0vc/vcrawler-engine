// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Crawler.Common;
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class AddTagViewModel
    {
        #region Fields

        private RelayCommand saveCommand;

        #endregion

        #region Constructors

        public AddTagViewModel()
        {
            Tags = new ObservableCollection<ITag>();
        }

        #endregion

        #region Properties

        public IChannel ParentChannel { get; set; }

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

        private void Save(object obj)
        {
            var window = obj as Window;
            if (window == null)
            {
                return;
            }
            if (!ParentChannel.ChannelTags.Select(x => x.Title).Contains(SelectedTag.Title))
            {
                ParentChannel.ChannelTags.Add(SelectedTag);
            }

            window.Close();
        }

        #endregion
    }
}
