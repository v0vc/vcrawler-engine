// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Crawler.Common;
using DataAPI;
using Interfaces.Enums;
using Interfaces.Models;
using Interfaces.POCO;

namespace Crawler.ViewModels
{
    public class ServiceChannelViewModel : IChannel
    {
        #region Fields

        private RelayCommand fillPopularCommand;
        private MainWindowViewModel mainVm;
        private RelayCommand searchCommand;
        private CredImage selectedSite;
        private RelayCommand siteChangedCommand;

        #endregion

        #region Constructors

        public ServiceChannelViewModel()
        {
            Title = "#Popular";
            Countries = new[] { "RU", "US", "CA", "FR", "DE", "IT", "JP" };
            SelectedCountry = Countries.First();
            SupportedSites = new List<CredImage>();
            ChannelItems = new ObservableCollection<IVideoItem>();
            Site = SiteType.NotSet;
        }

        #endregion

        #region Properties

        public IEnumerable<string> Countries { get; private set; }

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

        public CredImage SelectedSite
        {
            get
            {
                return selectedSite;
            }
            set
            {
                if (Equals(value, selectedSite))
                {
                    return;
                }
                selectedSite = value;
            }
        }

        public RelayCommand SiteChangedCommand
        {
            get
            {
                return siteChangedCommand ?? (siteChangedCommand = new RelayCommand(x => SiteChanged()));
            }
        }

        public List<CredImage> SupportedSites { get; set; }

        #endregion

        #region Methods

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
            switch (SelectedSite.Cred.Site)
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

            switch (SelectedSite.Cred.Site)
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
                        SupportedSites.Add(new CredImage(cred, "Crawler.Images.pop.png"));
                        break;

                    case SiteType.RuTracker:

                        SupportedSites.Add(new CredImage(cred, "Crawler.Images.rt.png"));
                        break;

                    case SiteType.Tapochek:

                        SupportedSites.Add(new CredImage(cred, "Crawler.Images.tap.png"));
                        break;
                }
            }
            SelectedSite = SupportedSites.First();
        }

        private void SiteChanged()
        {
            mainVm.SelectedChannel = this;
        }

        #endregion

        #region IChannel Members

        public CookieContainer ChannelCookies { get; set; }
        public ObservableCollection<IVideoItem> ChannelItems { get; set; }
        public int ChannelItemsCount { get; set; }
        public ObservableCollection<IPlaylist> ChannelPlaylists { get; set; }
        public ObservableCollection<ITag> ChannelTags { get; set; }
        public int CountNew { get; set; }
        public string ID { get; set; }
        public bool IsDownloading { get; set; }
        public bool IsInWork { get; set; }
        public bool IsSelected { get; set; }
        public bool IsShowRow { get; set; }
        public int PlaylistCount { get; set; }
        public IList<IVideoItem> SelectedItems
        {
            get
            {
                return ChannelItems.Where(x => x.IsSelected).ToList();
            }
        }
        public SiteType Site { get; set; }
        public string SiteAdress { get; set; }
        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; set; }
        public string Title { get; set; }

        public void AddNewItem(IVideoItem item, bool isNew)
        {
            item.IsNewItem = isNew;
            item.IsShowRow = true;
            item.State = ItemState.LocalNo;
            item.IsHasLocalFile = false;

            if (isNew)
            {
                ChannelItems.Insert(0, item);
            }
            else
            {
                ChannelItems.Add(item);
            }
        }

        public Task DeleteChannelAsync()
        {
            throw new NotImplementedException();
        }

        public Task DeleteChannelTagAsync(string tag)
        {
            throw new NotImplementedException();
        }

        public void FillChannelCookieDb()
        {
            throw new NotImplementedException();
        }

        public Task FillChannelCookieNetAsync()
        {
            throw new NotImplementedException();
        }

        public Task FillChannelDescriptionAsync()
        {
            throw new NotImplementedException();
        }

        public Task FillChannelItemsDbAsync(string dir, int count, int offset)
        {
            throw new NotImplementedException();
        }

        public Task<ICred> GetChannelCredentialsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetChannelItemsCountDbAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetChannelItemsCountNetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IVideoItem>> GetChannelItemsDbAsync(int count, int offset)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetChannelItemsIdsListDbAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetChannelItemsIdsListNetAsync(int maxresult)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IVideoItem>> GetChannelItemsNetAsync(int maxresult)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetChannelPlaylistCountDbAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IPlaylist>> GetChannelPlaylistsDbAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IPlaylist>> GetChannelPlaylistsNetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ITag>> GetChannelTagsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IVideoItem>> GetPopularItemsNetAsync(string regionID, int maxresult)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IChannel>> GetRelatedChannelNetAsync(string id, SiteType site)
        {
            throw new NotImplementedException();
        }

        public Task InsertChannelAsync()
        {
            throw new NotImplementedException();
        }

        public Task InsertChannelItemsAsync()
        {
            throw new NotImplementedException();
        }

        public Task InsertChannelTagAsync(string tag)
        {
            throw new NotImplementedException();
        }

        public Task RenameChannelAsync(string newName)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IVideoItem>> SearchItemsNetAsync(string key, string region, int maxresult)
        {
            throw new NotImplementedException();
        }

        public void StoreCookies()
        {
            throw new NotImplementedException();
        }

        public Task SyncChannelAsync(bool isSyncPls)
        {
            throw new NotImplementedException();
        }

        public Task SyncChannelPlaylistsAsync()
        {
            throw new NotImplementedException();
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
