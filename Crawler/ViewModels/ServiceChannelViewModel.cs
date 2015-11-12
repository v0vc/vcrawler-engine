// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Crawler.Common;
using DataAPI;
using Interfaces.Enums;
using Interfaces.Models;
using Interfaces.POCO;

namespace Crawler.ViewModels
{
    public class ServiceChannelViewModel
    {
        #region Fields

        private RelayCommand fillPopularCommand;
        private MainWindowViewModel mainVm;
        private RelayCommand searchCommand;

        #endregion

        #region Constructors

        public ServiceChannelViewModel()
        {
            Title = "#Popular";
            Countries = new[] { "RU", "US", "CA", "FR", "DE", "IT", "JP" };
            SelectedCountry = Countries.First();
            SupportedCreds = new List<CredImage>();
            ChannelItems = new ObservableCollection<IVideoItem>();
        }

        #endregion

        #region Properties

        public ObservableCollection<IVideoItem> ChannelItems { get; set; }
        public IEnumerable<string> Countries { get; set; }

        public RelayCommand FillPopularCommand
        {
            get
            {
                return fillPopularCommand ?? (fillPopularCommand = new RelayCommand(async x => await FillPopular()));
            }
        }

        public RelayCommand SearchCommand
        {
            get
            {
                return searchCommand ?? (searchCommand = new RelayCommand(async x => await Search()));
            }
        }

        public string SearchKey { get; set; }
        public string SelectedCountry { get; set; }
        public CredImage SelectedCredImage { get; set; }
        public List<CredImage> SupportedCreds { get; set; }
        public string Title { get; set; }

        #endregion

        #region Methods

        public void AddNewItem(IVideoItem item, bool isNew)
        {
            item.IsNewItem = isNew;
            item.IsShowRow = true;
            item.State = ItemState.LocalNo;
            item.IsHasLocalFile = false;
            item.Site = item.Site;

            if (isNew)
            {
                ChannelItems.Insert(0, item);
            }
            else
            {
                ChannelItems.Add(item);
            }
        }

        public async Task FillPopular()
        {
            if (ChannelItems.Any())
            {
                // чтоб не удалять список отдельных закачек, но почистить прошлые популярные
                for (int i = ChannelItems.Count; i > 0; i--)
                {
                    if (!(ChannelItems[i - 1].State == ItemState.LocalYes || ChannelItems[i - 1].State == ItemState.Downloading))
                    {
                        ChannelItems.RemoveAt(i - 1);
                    }
                }
            }

            mainVm.SetStatus(1);
            switch (SelectedCredImage.Cred.Site)
            {
                case SiteType.YouTube:

                    IEnumerable<IVideoItemPOCO> lst =
                        await mainVm.BaseFactory.CreateYouTubeSite().GetPopularItemsAsync(SelectedCountry, 30);
                    foreach (IVideoItemPOCO poco in lst)
                    {
                        IVideoItem item = mainVm.BaseFactory.CreateVideoItemFactory().CreateVideoItem(poco);
                        AddNewItem(item, false);
                        item.IsHasLocalFileFound(mainVm.SettingsViewModel.DirPath);
                        if (mainVm.Channels.Select(x => x.ID).Contains(item.ParentID))
                        {
                            // подсветим видео, если канал уже есть в подписке
                            item.IsNewItem = true;
                        }
                    }

                    break;
            }

            mainVm.SetStatus(0);
        }

        public void Init(MainWindowViewModel mainWindowModel)
        {
            mainVm = mainWindowModel;
            FillCredImages();
        }

        public async Task Search()
        {
            if (string.IsNullOrEmpty(SearchKey))
            {
                return;
            }

            mainVm.SetStatus(1);

            switch (SelectedCredImage.Cred.Site)
            {
                case SiteType.YouTube:

                    List<IVideoItemPOCO> lst =
                        (await mainVm.BaseFactory.CreateYouTubeSite().SearchItemsAsync(SearchKey, SelectedCountry, 50)).ToList();
                    if (lst.Any())
                    {
                        for (int i = ChannelItems.Count; i > 0; i--)
                        {
                            if (!(ChannelItems[i - 1].State == ItemState.LocalYes || ChannelItems[i - 1].State == ItemState.Downloading))
                            {
                                ChannelItems.RemoveAt(i - 1);
                            }
                        }
                        foreach (IVideoItemPOCO poco in lst)
                        {
                            IVideoItem item = mainVm.BaseFactory.CreateVideoItemFactory().CreateVideoItem(poco);
                            AddNewItem(item, false);
                            item.IsHasLocalFileFound(mainVm.SettingsViewModel.DirPath);
                        }
                    }
                    break;
            }

            // SelectedChannel = channel;
            mainVm.SelectedChannel.ChannelItemsCount = ChannelItems.Count;
            mainVm.SetStatus(0);
        }

        private void FillCredImages()
        {
            foreach (ICred cred in mainVm.SettingsViewModel.SupportedCreds.Where(x => x.Site != SiteType.NotSet))
            {
                switch (cred.Site)
                {
                    case SiteType.YouTube:
                        SupportedCreds.Add(new CredImage(cred, "Crawler.Images.pop.png"));
                        break;

                    case SiteType.RuTracker:

                        // SupportedCreds.Add(new CredImage(cred, "Crawler.Images.rt.png"));
                        break;

                    case SiteType.Tapochek:

                        // SupportedCreds.Add(new CredImage(cred, "Crawler.Images.tap.png"));
                        break;
                }
            }
            SelectedCredImage = SupportedCreds.First();
        }

        #endregion

        #region Nested type: CredImage

        public class CredImage
        {
            #region Constructors

            public CredImage(ICred cred, string resourcepic)
            {
                Cred = cred;
                Stream img = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcepic);
                Thumbnail = SiteHelper.ReadFully(img);
            }

            #endregion

            #region Properties

            public ICred Cred { get; set; }
            public byte[] Thumbnail { get; set; }

            #endregion
        }

        #endregion
    }
}
