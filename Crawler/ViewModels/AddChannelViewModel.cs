// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Crawler.Common;
using Interfaces.Models;
using Models.BO;

namespace Crawler.ViewModels
{
    public class AddChannelViewModel
    {
        #region Fields

        private RelayCommand saveNewItemCommand;

        #endregion

        #region Constructors

        public AddChannelViewModel()
        {
            // for xaml
        }

        public AddChannelViewModel(bool isEditMode, MainWindowViewModel mvModel)
        {
            IsEditMode = isEditMode;
            MvModel = mvModel;

            if (IsEditMode)
            {
                ChannelLink = mvModel.SelectedChannel.ID;
                ChannelTitle = mvModel.SelectedChannel.Title;
                IsLinkEnabled = true;
                IsLinkReadonly = true;
                IsSitiesEnabled = false;
                if (MvModel.SettingsViewModel.SupportedCreds.Any())
                {
                    SelectedCred = MvModel.SettingsViewModel.SupportedCreds.FirstOrDefault(x => x.Site == MvModel.SelectedChannel.Site);
                }
            }
            else
            {
                var text = Clipboard.GetData(DataFormats.Text) as string;
                if (string.IsNullOrWhiteSpace(text) || text.Contains(Environment.NewLine))
                {
                    ChannelLink = RemoveSpecialCharacters(text);
                }
                else
                {
                    ChannelLink = text;
                }

                ChannelTitle = string.Empty;
                IsLinkEnabled = true;
                IsLinkReadonly = false;
                IsSitiesEnabled = true;

                if (MvModel.SettingsViewModel.SupportedCreds.Any())
                {
                    SelectedCred = MvModel.SettingsViewModel.SupportedCreds.First();
                }
            }
        }

        #endregion

        #region Properties

        public string ButtonContent
        {
            get
            {
                return IsEditMode ? "SAVE" : "OK";
            }
        }

        public string TitleContent
        {
            get
            {
                return IsEditMode ? "Edit" : "Add";
            }
        }

        public string ChannelLink { get; set; }
        public string ChannelTitle { get; set; }
        public bool IsEditMode { get; private set; }
        public bool IsLinkEnabled { get; set; }

        public bool IsLinkReadonly { get; set; }
        public bool IsSitiesEnabled { get; set; }

        public MainWindowViewModel MvModel { get; set; }

        public RelayCommand SaveNewItemCommand
        {
            get
            {
                return saveNewItemCommand ?? (saveNewItemCommand = new RelayCommand(SaveChannel));
            }
        }

        public ICred SelectedCred { get; set; }

        #endregion

        #region Static Methods

        private static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", string.Empty, RegexOptions.Compiled);
        }

        #endregion

        #region Methods

        private async void SaveChannel(object obj)
        {
            var window = obj as Window;
            if (window == null)
            {
                return;
            }
            window.Close();

            if (IsEditMode)
            {
                MvModel.SelectedChannel.Title = ChannelTitle;
                var channel = MvModel.SelectedChannel as Channel;
                if (channel != null)
                {
                    await channel.RenameChannelAsync(ChannelTitle);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(ChannelLink))
                {
                    MessageBox.Show("Fill channel link");
                    return;
                }
                try
                {
                    await MvModel.AddNewChannel(ChannelLink, ChannelTitle, SelectedCred.Site);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        #endregion
    }
}
