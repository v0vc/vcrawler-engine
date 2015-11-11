// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Linq;
using System.Windows;
using Crawler.Common;
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class AddNewTagViewModel
    {
        #region Static and Readonly Fields

        private readonly SettingsViewModel smModel;

        #endregion

        #region Fields

        private RelayCommand addTagCommand;

        #endregion

        #region Constructors

        public AddNewTagViewModel()
        {
            // for xaml
        }

        public AddNewTagViewModel(SettingsViewModel smModel)
        {
            this.smModel = smModel;
        }

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
            if (smModel.SupportedTags.Select(x => x.Title).Contains(Tag.Title))
            {
                return;
            }
            smModel.SupportedTags.Add(Tag);
            await Tag.InsertTagAsync();

            window.Close();
        }

        #endregion
    }
}
