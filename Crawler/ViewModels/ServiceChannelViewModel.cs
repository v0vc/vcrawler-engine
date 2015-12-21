// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using Crawler.Common;
using DataAPI;
using Interfaces.Enums;
using Interfaces.Models;
using Interfaces.POCO;

namespace Crawler.ViewModels
{
    public class ServiceChannelViewModel : IChannel, INotifyPropertyChanged
    {
        #region Static and Readonly Fields

        private readonly Dictionary<string, List<IVideoItem>> popCountriesDictionary;

        #endregion

        #region Fields

        private RelayCommand fillPopularCommand;
        private string filterVideoKey;
        private MainWindowViewModel mainVm;
        private RelayCommand searchCommand;
        private string selectedCountry;
        private CredImage selectedSite;
        private RelayCommand siteChangedCommand;

        #endregion

        #region Constructors

        public ServiceChannelViewModel()
        {
            Title = "#Popular";
            Countries = new[] { "RU", "US", "CA", "FR", "DE", "IT", "JP" };
            popCountriesDictionary = new Dictionary<string, List<IVideoItem>>();
            SelectedCountry = Countries.First();
            SupportedSites = new List<CredImage>();
            ChannelItems = new ObservableCollection<IVideoItem>();
            Site = SiteType.NotSet;
            ChannelItemsCollectionView = CollectionViewSource.GetDefaultView(ChannelItems);
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

        public string SelectedCountry
        {
            get
            {
                return selectedCountry;
            }
            set
            {
                if (value == selectedCountry)
                {
                    return;
                }
                selectedCountry = value;
                OnPropertyChanged();
                List<IVideoItem> lst;
                if (!popCountriesDictionary.TryGetValue(selectedCountry, out lst))
                {
                    return;
                }
                if (!lst.Any())
                {
                    return;
                }
                ChannelItems.Clear();
                foreach (IVideoItem item in lst)
                {
                    AddNewItem(item);
                }
            }
        }

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
                            if (
                                !(ChannelItems[i - 1].FileState == ItemState.LocalYes
                                  || ChannelItems[i - 1].FileState == ItemState.Downloading))
                            {
                                ChannelItems.RemoveAt(i - 1);
                            }
                        }
                        foreach (IVideoItem item in lst.Select(poco => mainVm.BaseFactory.CreateVideoItemFactory().CreateVideoItem(poco)))
                        {
                            AddNewItem(item);
                            item.IsHasLocalFileFound(mainVm.SettingsViewModel.DirPath);
                        }
                    }
                    break;
            }

            // SelectedChannel = channel;
            mainVm.SelectedChannel.ChannelItemsCount = ChannelItems.Count;
            mainVm.SetStatus(0);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
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

        private async Task FillPopular()
        {
            if (ChannelItems.Any())
            {
                // чтоб не удалять список отдельных закачек, но почистить прошлые популярные
                for (int i = ChannelItems.Count; i > 0; i--)
                {
                    if (!(ChannelItems[i - 1].FileState == ItemState.LocalYes || ChannelItems[i - 1].FileState == ItemState.Downloading))
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
                    var lstemp = new List<IVideoItem>();
                    foreach (IVideoItemPOCO poco in lst)
                    {
                        IVideoItem item = mainVm.BaseFactory.CreateVideoItemFactory().CreateVideoItem(poco);
                        AddNewItem(item);
                        item.IsHasLocalFileFound(mainVm.SettingsViewModel.DirPath);
                        if (mainVm.Channels.Select(x => x.ID).Contains(item.ParentID))
                        {
                            // подсветим видео, если канал уже есть в подписке
                            item.SyncState = SyncState.Added;
                        }
                        lstemp.Add(item);
                    }
                    if (popCountriesDictionary.ContainsKey(SelectedCountry))
                    {
                        popCountriesDictionary.Remove(SelectedCountry);
                    }
                    popCountriesDictionary.Add(SelectedCountry, lstemp);
                    break;
            }

            mainVm.SetStatus(0);
        }

        private bool FilterVideo(object item)
        {
            var value = (IVideoItem)item;
            if (value == null || value.Title == null)
            {
                return false;
            }

            return value.Title.ToLower().Contains(FilterVideoKey.ToLower());
        }

        private void SiteChanged()
        {
            mainVm.SelectedChannel = this;
        }

        #endregion

        #region IChannel Members

        public CookieContainer ChannelCookies { get; set; }
        public ObservableCollection<IVideoItem> ChannelItems { get; set; }
        public ICollectionView ChannelItemsCollectionView { get; set; }
        public int ChannelItemsCount { get; set; }
        public ObservableCollection<IPlaylist> ChannelPlaylists { get; set; }
        public ObservableCollection<ITag> ChannelTags { get; set; }
        public int CountNew { get; set; }

        public string FilterVideoKey
        {
            get
            {
                return filterVideoKey;
            }
            set
            {
                if (value == filterVideoKey)
                {
                    return;
                }

                filterVideoKey = value;
                ChannelItemsCollectionView.Filter = FilterVideo;
                OnPropertyChanged();
            }
        }

        public string ID { get; set; }
        public ChannelState ChannelState { get; set; }
        public int PlaylistCount { get; set; }
        public IVideoItem SelectedItem { get; set; }

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
        public string DirPath { get; set; }
        public bool IsShowSynced { get; set; }

        public void AddNewItem(IVideoItem item)
        {
            item.FileState = ItemState.LocalNo;

            if (item.SyncState == SyncState.Added)
            {
                item.SyncState = SyncState.Added;
                ChannelItems.Insert(0, item);
            }
            else
            {
                item.SyncState = SyncState.Notset;
                ChannelItems.Add(item);
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

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
