// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Crawler.Common;
using DataAPI;
using Interfaces.Enums;
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class EditDescriptionViewModel : INotifyPropertyChanged
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

        public EditDescriptionViewModel()
        {
            // for xaml
        }

        public EditDescriptionViewModel(IVideoItem item)
        {
            this.item = item;
            Title = item.Title;
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
            set
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
            set
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
            set
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private async Task FillData()
        {
            if (string.IsNullOrEmpty(item.Description))
            {
                await item.FillDescriptionAsync();
            }

            Description = item.Description;

            string link = null;

            switch (item.Site)
            {
                case SiteType.YouTube:
                    link = string.Format("http://img.youtube.com/vi/{0}/0.jpg", item.ID);
                    break;
            }

            LargeThumb = await SiteHelper.GetStreamFromUrl(link);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
