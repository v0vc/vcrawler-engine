// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Crawler.Common;
using Extensions.Helpers;
using Interfaces.Models;
using Models.BO.Items;

namespace Crawler.ViewModels
{
    public sealed class EditDescriptionViewModel : INotifyPropertyChanged
    {
        #region Static and Readonly Fields

        private readonly IVideoItem item;

        #endregion

        #region Fields

        private string description;
        private RelayCommand fillDataCommand;
        private byte[] largeThumb;
        private string title;

        #endregion

        #region Constructors

        public EditDescriptionViewModel(string chtitle, IVideoItem item)
        {
            this.item = item;
            Title = string.Format("{0} : {1}", chtitle, item.Title);
            Description = item.Description;
        }

        #endregion

        #region Properties

        public string Description
        {
            get
            {
                return description;
            }
            private set
            {
                if (value == description)
                {
                    return;
                }
                description = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand FillDataCommand
        {
            get
            {
                return fillDataCommand ?? (fillDataCommand = new RelayCommand(async x => await FillData()));
            }
        }

        public byte[] LargeThumb
        {
            get
            {
                return largeThumb;
            }
            private set
            {
                if (value == largeThumb)
                {
                    return;
                }
                largeThumb = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get
            {
                return title;
            }
            private set
            {
                if (value == title)
                {
                    return;
                }
                title = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        private async Task FillData()
        {
            if (string.IsNullOrEmpty(item.Description))
            {
                await item.FillDescriptionAsync().ConfigureAwait(false);
            }

            Description = item.Description;

            string link = null;

            if (item is YouTubeItem)
            {
                link = string.Format("http://img.youtube.com/vi/{0}/0.jpg", item.ID);
            }

            if (link != null)
            {
                LargeThumb = await SiteHelper.GetStreamFromUrl(link).ConfigureAwait(false);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
