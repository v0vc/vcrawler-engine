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
using DataAPI.Database;
using DataAPI.POCO;
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
        private readonly List<IVideoItem> hasfileList;
        private readonly List<IVideoItem> plannedList;

        private readonly Dictionary<string, object> supportedStates = new Dictionary<string, object>
        {
            { "Crawler.Images.new_48.png", SyncState.Added },
            { "Crawler.Images.time_48.png", WatchState.Planned },
            { "Crawler.Images.tick_48.png", WatchState.Watched },
            { "Crawler.Images.done_48.png", ItemState.LocalYes }
        };

        private readonly List<IVideoItem> watchedList;

        #endregion

        #region Fields

        private List<string> addedListIds;
        private ObservableCollection<IChannel> allchannels;
        private int channelItemsCount;
        private List<string> plannedListIds;
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
            addedList = new List<IVideoItem>();
            plannedList = new List<IVideoItem>();
            watchedList = new List<IVideoItem>();
            hasfileList = new List<IVideoItem>();

            SupportedStates = new List<StateImage>();
            foreach (KeyValuePair<string, object> pair in supportedStates)
            {
                SupportedStates.Add(new StateImage(pair.Value, pair.Key));
            }
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

        public List<StateImage> SupportedStates { get; private set; }

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
                        }

                        break;
                    case WatchState.Planned:
                        if (!plannedList.Select(x => x.ID).Contains(item.ID))
                        {
                            plannedList.Add(item);
                            IVideoItem vi = watchedList.FirstOrDefault(x => x.ID == item.ID);
                            if (vi != null)
                            {
                                watchedList.Remove(vi);
                            }
                        }

                        break;

                    case WatchState.Notset:
                        IVideoItem vim = plannedList.FirstOrDefault(x => x.ID == item.ID);
                        if (vim != null)
                        {
                            plannedList.Remove(vim);
                        }
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
                        }
                        break;

                    case SyncState.Notset:
                        IVideoItem vi = addedList.FirstOrDefault(x => x.ID == item.ID);
                        if (vi != null)
                        {
                            addedList.Remove(vi);
                        }
                        break;
                }
            }
            else if (state is ItemState)
            {
                var st = (ItemState)state;
                if (st == ItemState.LocalYes)
                {
                    if (!hasfileList.Contains(item))
                    {
                        hasfileList.Add(item);
                    }
                }
            }
        }

        public void ClearAddedAllList()
        {
            addedList.Clear();
            if (SelectedState.State is ItemState)
            {
                ChannelItems.Clear();
            }
        }

        public void Init(ObservableCollection<IChannel> channels)
        {
            allchannels = channels;
            SelectedState = SupportedStates.First();
        }

        private async void InitIds()
        {
            addedListIds = await Task.Run(() => db.GetWatchStateListItemsAsync(SyncState.Added));
            plannedListIds = await Task.Run(() => db.GetWatchStateListItemsAsync(WatchState.Planned));
            watchedListIds = await Task.Run(() => db.GetWatchStateListItemsAsync(WatchState.Watched));
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
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
                List<string> readyAddedIds;
                List<string> notreadyList;
                List<VideoItemPOCO> items;
                switch (st)
                {
                    case WatchState.Watched:
                        readyAddedIds =
                            allchannels.SelectMany(x => x.ChannelItems)
                                .Where(y => y.WatchState == WatchState.Watched)
                                .Select(x => x.ID)
                                .ToList();
                        foreach (string id in watchedList.Select(x => x.ID).Where(id => !readyAddedIds.Contains(id)))
                        {
                            readyAddedIds.Add(id);
                        }
                        notreadyList = watchedListIds.Except(readyAddedIds).ToList();
                        if (notreadyList.Any())
                        {
                            items = await Task.Run(() => db.GetItemsByIdsAndState(WatchState.Watched, notreadyList));
                            foreach (VideoItemPOCO poco in items)
                            {
                                IChannel parent = allchannels.First(x => x.ID == poco.ParentID);
                                IVideoItem item = VideoItemFactory.CreateVideoItem(poco, parent.Site);
                                parent.AddNewItem(item, false);
                                parent.IsHasNewFromSync = true;
                                watchedList.Add(item);
                            }
                        }
                        watchedList.ForEach(x => AddNewItem(x));

                        break;
                    case WatchState.Planned:

                        readyAddedIds =
                            allchannels.SelectMany(x => x.ChannelItems)
                                .Where(y => y.WatchState == WatchState.Planned)
                                .Select(x => x.ID)
                                .ToList();
                        foreach (string id in plannedList.Select(x => x.ID).Where(id => !readyAddedIds.Contains(id)))
                        {
                            readyAddedIds.Add(id);
                        }
                        notreadyList = plannedListIds.Except(readyAddedIds).ToList();
                        if (notreadyList.Any())
                        {
                            items = await Task.Run(() => db.GetItemsByIdsAndState(WatchState.Planned, notreadyList));
                            foreach (VideoItemPOCO poco in items)
                            {
                                IChannel parent = allchannels.First(x => x.ID == poco.ParentID);
                                IVideoItem item = VideoItemFactory.CreateVideoItem(poco, parent.Site);
                                parent.AddNewItem(item, false);
                                parent.IsHasNewFromSync = true;
                                plannedList.Add(item);
                            }
                        }
                        plannedList.ForEach(x => AddNewItem(x));

                        break;
                }
            }
            else if (state is SyncState)
            {
                var st = (SyncState)state;
                switch (st)
                {
                    case SyncState.Added:

                        List<string> readyAddedIds =
                            allchannels.SelectMany(x => x.ChannelItems)
                                .Where(y => y.SyncState == SyncState.Added)
                                .Select(x => x.ID)
                                .ToList();
                        foreach (string id in addedList.Select(x => x.ID).Where(id => !readyAddedIds.Contains(id)))
                        {
                            readyAddedIds.Add(id);
                        }
                        List<string> notreadyList = addedListIds.Except(readyAddedIds).ToList();
                        if (notreadyList.Any())
                        {
                            List<VideoItemPOCO> items = await Task.Run(() => db.GetItemsByIdsAndState(SyncState.Added, notreadyList));
                            foreach (VideoItemPOCO poco in items)
                            {
                                IChannel parent = allchannels.First(x => x.ID == poco.ParentID);
                                IVideoItem item = VideoItemFactory.CreateVideoItem(poco, parent.Site);
                                parent.AddNewItem(item, false);
                                parent.IsHasNewFromSync = true;
                                addedList.Add(item);
                            }
                        }
                        addedList.ForEach(x => AddNewItem(x));
                        break;
                }
            }
            else if (state is ItemState)
            {
                var st = (ItemState)state;
                switch (st)
                {
                    case ItemState.LocalYes:
                        hasfileList.ForEach(x => ChannelItems.Add(x));
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
                channelItemsCount = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IPlaylist> ChannelPlaylists { get; set; }
        public ChannelState ChannelState { get; set; }
        public ObservableCollection<ITag> ChannelTags { get; set; }
        public int CountNew { get; set; }
        public string DirPath { get; set; }
        public string FilterVideoKey { get; set; }
        public string ID { get; set; }
        public bool IsHasNewFromSync { get; set; }
        public bool IsShowSynced { get; set; }
        public int PlaylistCount { get; set; }
        public IVideoItem SelectedItem { get; set; }

        public SiteType Site
        {
            get
            {
                return SiteType.NotSet;
            }
        }

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

        public void AddNewItem(IVideoItem item, bool isIncrease = true)
        {
            item.IsHasLocalFileFound(DirPath);
            ChannelItems.Add(item);
            ChannelItemsCount += 1;
        }

        public void DeleteItem(IVideoItem item)
        {
            throw new NotImplementedException();
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

            public object State { get; private set; }

            public byte[] Thumbnail { get; private set; }

            #endregion
        }

        #endregion
    }
}
