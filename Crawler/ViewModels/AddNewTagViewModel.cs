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
    public class AddNewTagViewModel
    {
        #region Fields

        private RelayCommand addTagCommand;

        #endregion

        #region Properties

        public RelayCommand AddTagCommand
        {
            get
            {
                return addTagCommand ?? (addTagCommand = new RelayCommand(AddTag));
            }
        }

        public ITag Tag { get; set; }
        public ObservableCollection<ITag> Tags { private get; set; }

        #endregion

        #region Methods

        private async void AddTag(object obj)
        {
            var window = obj as Window;
            if (window == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(Tag.Title))
            {
                return;
            }
            if (Tags.Select(x => x.Title).Contains(Tag.Title))
            {
                return;
            }
            Tags.Add(Tag);
            await Tag.InsertTagAsync();
            Tag.Title = string.Empty;
            window.Close();
        }

        #endregion
    }
}
