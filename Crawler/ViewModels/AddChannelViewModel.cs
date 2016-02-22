// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Crawler.Common;
using DataAPI.Database;
using Extensions;
using Interfaces.Enums;
using Interfaces.Models;
using Models.Factories;

namespace Crawler.ViewModels
{
    public class AddChannelViewModel
    {
        #region Static and Readonly Fields

        private readonly IChannel channel;
        private readonly Action<string, string, SiteType> onAddNewChannel;

        #endregion

        #region Fields

        private RelayCommand saveNewItemCommand;

        #endregion

        #region Constructors

        public AddChannelViewModel(bool isEditMode,
            List<ICred> supportedCreds,
            Action<string, string, SiteType> onAddNewChannel = null,
            IChannel channel = null)
        {
            this.channel = channel;
            this.onAddNewChannel = onAddNewChannel;
            IsEditMode = isEditMode;
            SupportedCreds = supportedCreds;

            if (IsEditMode && channel != null)
            {
                ChannelLink = channel.ID;
                ChannelTitle = channel.Title;
                UseFast = channel.UseFast;
                if (SupportedCreds.Any())
                {
                    SelectedCred = SupportedCreds.FirstOrDefault(x => x.Site == channel.Site);
                }
            }
            else
            {
                var text = Clipboard.GetData(DataFormats.Text) as string;
                if (string.IsNullOrWhiteSpace(text) || text.Contains(Environment.NewLine))
                {
                    ChannelLink = text.RemoveSpecialCharacters();
                }
                else
                {
                    ChannelLink = text;
                }
                ChannelTitle = string.Empty;
                if (SupportedCreds.Any())
                {
                    SelectedCred = SupportedCreds.First();
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

        public string ChannelLink { get; set; }
        public string ChannelTitle { get; set; }
        public bool IsEditMode { get; private set; }

        public RelayCommand SaveNewItemCommand
        {
            get
            {
                return saveNewItemCommand ?? (saveNewItemCommand = new RelayCommand(SaveChannel));
            }
        }

        public ICred SelectedCred { get; set; }
        public List<ICred> SupportedCreds { get; set; }

        public string TitleContent
        {
            get
            {
                return IsEditMode ? "Edit" : "Add";
            }
        }

        public bool UseFast { get; set; }

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

            if (IsEditMode && channel != null)
            {
                SqLiteDatabase db = CommonFactory.CreateSqLiteDatabase();
                channel.Title = ChannelTitle;
                await db.RenameChannelAsync(channel.ID, ChannelTitle).ConfigureAwait(false);
                if (Equals(channel.UseFast, UseFast))
                {
                    return;
                }
                channel.UseFast = UseFast;
                await db.UpdateChannelFastSync(channel.ID, channel.UseFast).ConfigureAwait(false);
            }
            else
            {
                if (string.IsNullOrEmpty(ChannelLink))
                {
                    MessageBox.Show("Fill channel link");
                    return;
                }
                if (onAddNewChannel != null)
                {
                    onAddNewChannel.Invoke(ChannelLink, ChannelTitle, SelectedCred.Site);
                }
            }
        }

        #endregion
    }
}
