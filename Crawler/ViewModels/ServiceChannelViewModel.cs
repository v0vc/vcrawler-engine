// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
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
using DataAPI.Database;
using DataAPI.POCO;
using DataAPI.Videos;
using Extensions;
using Extensions.Helpers;
using Interfaces.Enums;
using Interfaces.Models;
using Models.BO.Channels;
using Models.Factories;

namespace Crawler.ViewModels
{
    public sealed class ServiceChannelViewModel : CommonChannel, IChannel, INotifyPropertyChanged
    {
        #region Constants

        private const string dlindex = "DL";

        #endregion

        #region Static and Readonly Fields

        private readonly IEnumerable<string> countrieslist = new[] { "RU", "US", "CA", "FR", "DE", "IT", "JP", "UA", "SG", dlindex };
        private SqLiteDatabase db;
        #endregion

        #region Fields

        private string filterVideoKey;
        private KeyValuePair<string, List<IVideoItem>> selectedCountry;
        private IVideoItem selectedItem;
        private CredImage selectedSite;
        private string channelStatistics;
        private string selectedStat;

        #endregion

        #region Constructors

        public ServiceChannelViewModel()
        {
            Title = "#Popular";
            ChannelPlaylists = new ObservableCollection<IPlaylist>();
            SupportedSites = new List<CredImage>();
            ChannelItems = new ObservableCollection<IVideoItem>();
            ChannelItemsCollectionView = CollectionViewSource.GetDefaultView(ChannelItems);
            Countries = new Dictionary<string, List<IVideoItem>>();
            countrieslist.ForEach(x => Countries.Add(x, new List<IVideoItem>()));
            SelectedCountry = Countries.First();
            VideoTags = new ObservableCollection<ITag>();
        }

        #endregion

        #region Properties

        public Dictionary<string, List<IVideoItem>> Countries { get; }
        public string SearchKey { get; set; }

        public KeyValuePair<string, List<IVideoItem>> SelectedCountry
        {
            get
            {
                return selectedCountry;
            }
            set
            {
                selectedCountry = value;
                OnPropertyChanged();
                RefreshItems();
            }
        }

        public CredImage SelectedSite
        {
            get
            {
                return selectedSite;
            }
            private set
            {
                if (Equals(value, selectedSite))
                {
                    return;
                }
                selectedSite = value;
            }
        }

        public List<CredImage> SupportedSites { get; }

        #endregion

        #region Methods

        public void AddItemToDownload(IVideoItem item)
        {
            SelectedCountry = Countries.First(x => x.Key == dlindex);
            if (!SelectedCountry.Value.Select(x => x.ID).Contains(item.ID))
            {
                SelectedCountry.Value.Add(item);
            }
            RefreshItems();
        }

        public async Task FillPopular(Dictionary<string, string> dic)
        {
            if (SelectedCountry.Key == dlindex)
            {
                return;
            }

            SelectedCountry.Value.Clear();

            switch (SelectedSite.Cred.Site)
            {
                case SiteType.YouTube:

                    List<VideoItemPOCO> lst = await YouTubeSite.GetPopularItemsAsync(SelectedCountry.Key, 30).ConfigureAwait(true);

                    if (lst.Any())
                    {
                        foreach (IVideoItem item in lst.Select(poco => VideoItemFactory.CreateVideoItem(poco, SiteType.YouTube)))
                        {
                            item.IsHasLocalFileFound(DirPath);
                            string title;
                            if (dic.TryGetValue(item.ParentID, out title))
                            {
                                // подсветим видео, если канал уже есть в подписке
                                item.SyncState = SyncState.Added;
                                item.ParentTitle = title;
                                item.WatchState = await db.GetItemWatchStateAsync(item.ID).ConfigureAwait(false);
                            }
                            SelectedCountry.Value.Add(item);
                        }
                        RefreshItems();
                    }

                    break;
            }
            RefreshView("ViewCount");
        }

        public void Init(IEnumerable<ICred> supportedCreds, string dirPath, SqLiteDatabase dbase)
        {
            db = dbase;
            DirPath = dirPath;

            foreach (ICred cred in supportedCreds.Where(x => x.Site != SiteType.NotSet))
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

        public async Task Search(Dictionary<string, string> dic)
        {
            if (string.IsNullOrEmpty(SearchKey))
            {
                return;
            }

            switch (SelectedSite.Cred.Site)
            {
                case SiteType.YouTube:

                    if (SelectedCountry.Key == dlindex)
                    {
                        SelectedCountry = Countries.First();
                    }

                    SelectedCountry.Value.Clear();

                    List<VideoItemPOCO> lst = await YouTubeSite.SearchItemsAsync(SearchKey, SelectedCountry.Key, 50).ConfigureAwait(true);
                    if (lst.Any())
                    {
                        foreach (IVideoItem item in lst.Select(poco => VideoItemFactory.CreateVideoItem(poco, SiteType.YouTube)))
                        {
                            item.IsHasLocalFileFound(DirPath);
                            string title;
                            if (dic.TryGetValue(item.ParentID, out title))
                            {
                                // подсветим видео, если канал уже есть в подписке
                                item.SyncState = SyncState.Added;
                                item.ParentTitle = title;
                                item.WatchState = await db.GetItemWatchStateAsync(item.ID).ConfigureAwait(false);
                            }
                            SelectedCountry.Value.Add(item);
                        }
                        RefreshItems();
                    }
                    break;
            }
        }

        public void SiteChanged()
        {
            // TODO
        }

        private bool FilterVideo(object item)
        {
            var value = (IVideoItem)item;
            if (value?.Title == null)
            {
                return false;
            }

            return value.Title.ToLower().Contains(FilterVideoKey.ToLower());
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RefreshItems()
        {
            if (ChannelItems.Any())
            {
                ChannelItems.Clear();
            }
            SelectedCountry.Value.ForEach(x => AddNewItem(x));
        }

        #endregion

        #region IChannel Members

        public CookieContainer ChannelCookies { get; set; }
        public ObservableCollection<IVideoItem> ChannelItems { get; set; }
        public ICollectionView ChannelItemsCollectionView { get; set; }
        public int ChannelItemsCount { get; set; }
        public long ChannelViewCount { get; set; }
        public long ChannelLikeCount { get; set; }
        public long ChannelDislikeCount { get; set; }
        public long ChannelCommentCount { get; set; }
        public ObservableCollection<IPlaylist> ChannelPlaylists { get; set; }
        public ChannelState ChannelState { get; set; }
        public ObservableCollection<ITag> ChannelTags { get; set; }
        public string SelectedStat
        {
            get
            {
                return selectedStat;
            }
            set
            {
                if (value == selectedStat)
                {
                    return;
                }
                selectedStat = value;
                if (selectedStat == null)
                {
                    return;
                }
                ChannelItemsCollectionView.SortDescriptions.Clear();
                ChannelItemsCollectionView.SortDescriptions.Add(new SortDescription(Stats[selectedStat], ListSortDirection.Descending));
            }
        }
        public int CountNew { get; set; }
        public string DirPath { get; set; }
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
            }
        }
        public string ID { get; set; }
        public bool IsAllItems { get; set; }
        public bool IsHasNewFromSync { get; set; }
        public bool IsShowSynced { get; set; }
        public bool Loaded { get; set; }
        public int PlaylistCount { get; set; }
        public IVideoItem SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                if (Equals(value, selectedItem))
                {
                    return;
                }
                selectedItem = value;
                OnPropertyChanged();
            }
        }
        public SiteType Site => SiteType.NotSet;
        public string ChannelStatistics
        {
            get
            {
                return channelStatistics;
            }
            set
            {
                channelStatistics = value;
                OnPropertyChanged();
            }
        }
        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; set; }
        public string Title { get; set; }
        public bool UseFast { get; set; }
        public ObservableCollection<ITag> VideoTags { get; set; }

        public void AddNewItem(IVideoItem item, bool isIncrease = true, bool isUpdateCount = true)
        {
            if (item == null)
            {
                throw new ArgumentException("item");
            }

            if (item.ParentTitle == null)
            {
                item.ParentTitle = item.ParentID;
            }
            if (ChannelItems.Select(x => x.ID).Contains(item.ID))
            {
                ChannelItems.Remove(ChannelItems.First(x => x.ID == item.ID));
            }

            item.FileState = ItemState.LocalNo;

            if (item.SyncState == SyncState.Added)
            {
                ChannelItems.Insert(0, item);
            }
            else
            {
                ChannelItems.Add(item);
            }
        }

        public void DeleteItem(IVideoItem item)
        {
            if (item == null)
            {
                throw new ArgumentException("item");
            }

            ChannelItems.Remove(item);
        }

        public void RefreshView(string field)
        {
            ChannelItemsCollectionView.SortDescriptions.Clear();
            ChannelItemsCollectionView.SortDescriptions.Add(new SortDescription(field, ListSortDirection.Descending));
            ChannelItemsCollectionView.Refresh();
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
                if (img != null)
                {
                    Thumbnail = StreamHelper.ReadFully(img);
                }
            }

            #endregion

            #region Properties

            public ICred Cred { get; }
            public byte[] Thumbnail { get; private set; }

            #endregion
        }

        #endregion
    }
}
