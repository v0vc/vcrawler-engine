// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
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
using System.Windows.Data;
using DataAPI.Database;
using DataAPI.POCO;
using Extensions;
using Extensions.Helpers;
using Interfaces.Enums;
using Interfaces.Models;
using Models.Factories;

namespace Crawler.ViewModels
{
    public sealed class StateChannel : IChannel, INotifyPropertyChanged
    {
        #region Static and Readonly Fields

        private readonly List<IVideoItem> addedList;
        private readonly SqLiteDatabase db;
        private readonly List<IVideoItem> plannedList;

        private readonly Dictionary<string, object> supportedStates = new Dictionary<string, object>
        {
            { "Crawler.Images.new_48.png", SyncState.Added },
            { "Crawler.Images.time_48.png", WatchState.Planned },
            { "Crawler.Images.done_48.png", WatchState.Watched }
        };

        private readonly List<IVideoItem> watchedList;

        #endregion

        #region Fields

        private List<string> addedListIds;
        private ObservableCollection<IChannel> allchannels;
        private int channelItemsCount;
        private string filterVideoKey;
        private List<string> plannedListIds;
        private IVideoItem selectedItem;
        private StateImage selectedState;
        private string title;
        private List<string> watchedListIds;

        #endregion

        #region Constructors

        public StateChannel(SqLiteDatabase db)
        {
            this.db = db;
            ChannelPlaylists = new ObservableCollection<IPlaylist>();
            ChannelItems = new ObservableCollection<IVideoItem>();
            VideoTags = new ObservableCollection<ITag>();
            addedList = new List<IVideoItem>();
            plannedList = new List<IVideoItem>();
            watchedList = new List<IVideoItem>();

            SupportedStates = new List<StateImage>();
            foreach (KeyValuePair<string, object> pair in supportedStates)
            {
                SupportedStates.Add(new StateImage(pair.Value, pair.Key));
            }
            ChannelItemsCollectionView = CollectionViewSource.GetDefaultView(ChannelItems);
            InitIds();
        }

        #endregion

        #region Properties

        public StateImage SelectedState
        {
            get
            {
                return selectedState;
            }
            set
            {
                if (value.Equals(selectedState))
                {
                    return;
                }
                selectedState = value;
                OnPropertyChanged();
                OnStateChanged(selectedState);
            }
        }

        public List<StateImage> SupportedStates { get; }

        #endregion

        #region Methods

        public void AddToStateList(object state, IVideoItem item)
        {
            if (state is WatchState)
            {
                var st = (WatchState)state;
                switch (st)
                {
                    case WatchState.Watched:
                        if (!watchedList.Select(x => x.ID).Contains(item.ID))
                        {
                            watchedList.Add(item);
                            if (SelectedState != null && SelectedState.State as WatchState? == st)
                            {
                                AddNewItem(item);
                            }
                            if (!watchedListIds.Contains(item.ID))
                            {
                                watchedListIds.Add(item.ID);
                            }
                        }

                        break;
                    case WatchState.Planned:
                        if (!plannedList.Select(x => x.ID).Contains(item.ID))
                        {
                            plannedList.Add(item);
                            if (SelectedState != null && SelectedState.State as WatchState? == st)
                            {
                                AddNewItem(item);
                            }
                            if (!plannedListIds.Contains(item.ID))
                            {
                                plannedListIds.Add(item.ID);
                            }

                            IVideoItem vi = watchedList.FirstOrDefault(x => x.ID == item.ID);
                            if (vi != null)
                            {
                                watchedList.Remove(vi);
                                if (watchedListIds.Contains(item.ID))
                                {
                                    watchedListIds.Remove(item.ID);
                                }
                            }
                            if (SelectedState != null && SelectedState.State as WatchState? == WatchState.Watched)
                            {
                                DeleteItem(item);
                            }
                        }

                        break;

                    case WatchState.Notset:
                        IVideoItem vim = plannedList.FirstOrDefault(x => x.ID == item.ID);
                        if (vim != null)
                        {
                            plannedList.Remove(vim);
                            if (plannedListIds.Contains(item.ID))
                            {
                                plannedListIds.Remove(item.ID);
                            }
                        }

                        // if (SelectedState != null && SelectedState.State as WatchState? == WatchState.Planned)
                        // {
                        // DeleteItem(item);
                        // }
                        break;
                }
            }
            else if (state is SyncState)
            {
                var st = (SyncState)state;
                switch (st)
                {
                    case SyncState.Added:

                        if (!addedList.Select(x => x.ID).Contains(item.ID))
                        {
                            addedList.Add(item);
                            if (!addedListIds.Contains(item.ID))
                            {
                                addedListIds.Add(item.ID);
                                if (SelectedState != null && SelectedState.State as SyncState? == st)
                                {
                                    if (!ChannelItems.Select(x => x.ID).Contains(item.ID))
                                    {
                                        ChannelItems.Add(item);
                                    }
                                }
                            }
                        }

                        break;

                    case SyncState.Notset:
                        IVideoItem vi = addedList.FirstOrDefault(x => x.ID == item.ID);
                        if (vi != null)
                        {
                            addedList.Remove(vi);
                            if (addedListIds.Contains(item.ID))
                            {
                                addedListIds.Remove(item.ID);
                                if (SelectedState != null && SelectedState.State as SyncState? == SyncState.Added)
                                {
                                    IVideoItem ite = ChannelItems.FirstOrDefault(x => x.ID == item.ID);
                                    if (ite != null)
                                    {
                                        ChannelItems.Remove(ite);
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }

        public void Init(ObservableCollection<IChannel> channels)
        {
            allchannels = channels;
            SelectedState = SupportedStates.First();
        }

        private bool FilterVideoByTitle(object item)
        {
            var value = (IVideoItem)item;
            if (value?.Title == null)
            {
                return false;
            }

            return value.Title.ToLower().Contains(FilterVideoKey.ToLower());
        }

        private async void InitIds()
        {
            Dictionary<object, List<string>> dids = await db.GetStateListItemsAsync().ConfigureAwait(false);
            dids.TryGetValue(SyncState.Added, out addedListIds);
            dids.TryGetValue(WatchState.Watched, out watchedListIds);
            dids.TryGetValue(WatchState.Planned, out plannedListIds);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnStateChanged(StateImage stateImage)
        {
            Title = stateImage.State.ToString();
            ReloadFilteredLists(stateImage.State); 
        }

        private async void ReloadFilteredLists(object state)
        {
            ChannelItems.Clear();
            if (state is WatchState)
            {
                var st = (WatchState)state;
                List<IVideoItem> readyel;
                List<string> readyAddedIds;
                List<string> notreadyList;
                List<VideoItemPOCO> items;
                switch (st)
                {
                    case WatchState.Watched:

                        readyel = allchannels.SelectMany(x => x.ChannelItems).Where(y => y.WatchState == WatchState.Watched).ToList();
                        foreach (IVideoItem item in readyel.Where(item => !watchedList.Contains(item)))
                        {
                            watchedList.Add(item);
                        }
                        readyAddedIds = readyel.Select(x => x.ID).ToList();
                        foreach (string id in watchedList.Select(x => x.ID).Where(id => !readyAddedIds.Contains(id)))
                        {
                            readyAddedIds.Add(id);
                        }
                        notreadyList = watchedListIds.Except(readyAddedIds).ToList();
                        if (notreadyList.Any())
                        {
                            items = await db.GetItemsByIdsAndState(WatchState.Watched, notreadyList).ConfigureAwait(false);
                            foreach (VideoItemPOCO poco in items)
                            {
                                IChannel parent = allchannels.First(x => x.ID == poco.ParentID);
                                IVideoItem item = VideoItemFactory.CreateVideoItem(poco, parent.Site);
                                parent.AddNewItem(item, false);
                                parent.IsHasNewFromSync = true;
                                watchedList.Add(item);
                            }
                        }
                        watchedList.OrderBy(x => x.Timestamp).ForEach(x => AddNewItem(x));
                        break;
                    case WatchState.Planned:

                        readyel = allchannels.SelectMany(x => x.ChannelItems).Where(y => y.WatchState == WatchState.Planned).ToList();
                        foreach (IVideoItem item in readyel.Where(item => !plannedList.Contains(item)))
                        {
                            plannedList.Add(item);
                        }

                        readyAddedIds = readyel.Select(x => x.ID).ToList();
                        foreach (string id in plannedList.Select(x => x.ID).Where(id => !readyAddedIds.Contains(id)))
                        {
                            readyAddedIds.Add(id);
                        }
                        notreadyList = plannedListIds.Except(readyAddedIds).ToList();
                        if (notreadyList.Any())
                        {
                            items = await db.GetItemsByIdsAndState(WatchState.Planned, notreadyList).ConfigureAwait(false);
                            foreach (VideoItemPOCO poco in items)
                            {
                                IChannel parent = allchannels.First(x => x.ID == poco.ParentID);
                                IVideoItem item = VideoItemFactory.CreateVideoItem(poco, parent.Site);
                                parent.AddNewItem(item, false);
                                parent.IsHasNewFromSync = true;
                                plannedList.Add(item);
                            }
                        }
                        plannedList.OrderBy(x => x.Timestamp).ForEach(x => AddNewItem(x));
                        break;
                }
            }
            else if (state is SyncState)
            {
                var st = (SyncState)state;
                switch (st)
                {
                    case SyncState.Added:

                        List<IVideoItem> readyel =
                            allchannels.SelectMany(x => x.ChannelItems).Where(y => y.SyncState == SyncState.Added).ToList();
                        foreach (IVideoItem item in readyel.Where(item => !addedList.Contains(item)))
                        {
                            addedList.Add(item);
                        }
                        List<string> readyAddedIds = readyel.Select(x => x.ID).ToList();
                        foreach (string id in addedList.Select(x => x.ID).Where(id => !readyAddedIds.Contains(id)))
                        {
                            readyAddedIds.Add(id);
                        }
                        List<string> notreadyList = addedListIds.Except(readyAddedIds).ToList();
                        if (notreadyList.Any())
                        {
                            List<VideoItemPOCO> items =
                                await db.GetItemsByIdsAndState(SyncState.Added, notreadyList).ConfigureAwait(false);
                            foreach (VideoItemPOCO poco in items)
                            {
                                IChannel parent = allchannels.First(x => x.ID == poco.ParentID);
                                IVideoItem item = VideoItemFactory.CreateVideoItem(poco, parent.Site);
                                parent.AddNewItem(item, false);
                                parent.IsHasNewFromSync = true;
                                addedList.Add(item);
                            }
                        }
                        addedList.OrderBy(x => x.ParentID).ThenBy(y => y.Timestamp).ForEach(x => AddNewItem(x));
                        break;
                }
            }
        }

        #endregion

        #region IChannel Members

        public CookieContainer ChannelCookies { get; set; }
        public ObservableCollection<IVideoItem> ChannelItems { get; set; }
        public ICollectionView ChannelItemsCollectionView { get; set; }

        public int ChannelItemsCount
        {
            get
            {
                return channelItemsCount;
            }
            set
            {
                if (value == channelItemsCount)
                {
                    return;
                }
                channelItemsCount = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IPlaylist> ChannelPlaylists { get; set; }
        public ChannelState ChannelState { get; set; }
        public ObservableCollection<ITag> ChannelTags { get; set; }
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
                ChannelItemsCollectionView.Filter = FilterVideoByTitle;
                OnPropertyChanged();
            }
        }

        public string ID { get; set; }
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
                if (value == selectedItem)
                {
                    return;
                }
                selectedItem = value;
                OnPropertyChanged();
            }
        }

        public SiteType Site => SiteType.NotSet;

        public string SubTitle { get; set; }
        public byte[] Thumbnail { get; set; }

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

        public bool UseFast { get; set; }
        public ObservableCollection<ITag> VideoTags { get; set; }

        public void AddNewItem(IVideoItem item, bool isIncrease = true, bool isUpdateCount = true)
        {
            if (ChannelItems.Contains(item))
            {
                return;
            }
            item.IsHasLocalFileFound(DirPath);
            ChannelItems.Insert(0, item);
            if (isUpdateCount)
            {
                ChannelItemsCount += 1;
            }
        }

        public void DeleteItem(IVideoItem item)
        {
            IVideoItem el = ChannelItems.FirstOrDefault(x => x.ID == item.ID);
            if (el != null)
            {
                ChannelItems.Remove(el);
            }
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

        #region Nested type: StateImage

        public class StateImage
        {
            #region Constructors

            public StateImage(object state, string resourcepic)
            {
                State = state;
                Stream img = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcepic);
                if (img != null)
                {
                    Thumbnail = StreamHelper.ReadFully(img);
                }
            }

            #endregion

            #region Properties

            public object State { get; }
            public byte[] Thumbnail { get; private set; }

            #endregion
        }

        #endregion
    }
}
