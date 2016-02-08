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
using DataAPI.Database;
using Extensions.Helpers;
using Interfaces.Enums;
using Interfaces.Models;

namespace Crawler.ViewModels
{
    public class StateChannel : IChannel, INotifyPropertyChanged
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

        private StateImage selectedState;
        private string title;

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
            //addedPlaylist = PlaylistFactory.CreatePlaylist(SiteType.NotSet);
            //plannedPlaylist = PlaylistFactory.CreatePlaylist(SiteType.NotSet);
            //watchedPlaylist = PlaylistFactory.CreatePlaylist(SiteType.NotSet);
            //InitServicePlaylists();

            SupportedStates = new List<StateImage>();
            foreach (KeyValuePair<string, object> pair in supportedStates)
            {
                SupportedStates.Add(new StateImage(pair.Value, pair.Key));
            }
            SelectedState = SupportedStates.First();
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
                OnStateChanged();
            }
        }

        public List<StateImage> SupportedStates { get; private set; }

        #endregion

        #region Static Methods

        private static bool ListsContentdEquals(IReadOnlyCollection<string> list1, IReadOnlyCollection<string> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }
            var ids = new HashSet<string>(list2);
            return list1.All(ids.Contains);
        }

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
                        if (!watchedList.Contains(item))
                        {
                            watchedList.Add(item);
                        }

                        break;
                    case WatchState.Planned:
                        if (!plannedList.Contains(item))
                        {
                            plannedList.Add(item);
                        }

                        if (watchedList.Contains(item))
                        {
                            watchedList.Remove(item);
                        }

                        break;

                    case WatchState.Notset:
                        if (plannedList.Contains(item))
                        {
                            plannedList.Remove(item);
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

                        if (!addedList.Contains(item))
                        {
                            addedList.Add(item);
                        }
                        break;

                    case SyncState.Notset:
                        if (addedList.Contains(item))
                        {
                            addedList.Remove(item);
                        }
                        break;
                }
            }
        }

        public void ClearList(object state)
        {
            if (state is WatchState)
            {
                var st = (WatchState)state;
                switch (st)
                {
                    case WatchState.Watched:
                        watchedList.Clear();
                        break;
                    case WatchState.Planned:
                        plannedList.Clear();
                        break;
                }
            }
            else if (state is SyncState)
            {
                var st = (SyncState)state;
                switch (st)
                {
                    case SyncState.Added:
                        addedList.Clear();
                        break;
                }
            }
        }

        public bool IsAllItemsExist(object state, List<string> ids)
        {
            if (state is WatchState)
            {
                var st = (WatchState)state;
                switch (st)
                {
                    case WatchState.Watched:
                        return ListsContentdEquals(watchedList.Select(x => x.ID).ToList(), ids);
                    case WatchState.Planned:
                        return ListsContentdEquals(plannedList.Select(x => x.ID).ToList(), ids);
                }
            }
            else if (state is SyncState)
            {
                var st = (SyncState)state;
                switch (st)
                {
                    case SyncState.Added:
                        return ListsContentdEquals(addedList.Select(x => x.ID).ToList(), ids);
                }
            }
            return false;
        }

        public void ReloadFilteredLists(object state)
        {
            ChannelItems.Clear();
            if (state is WatchState)
            {
                var st = (WatchState)state;
                switch (st)
                {
                    case WatchState.Watched:
                        watchedList.ForEach(x => ChannelItems.Add(x));
                        break;
                    case WatchState.Planned:
                        plannedList.ForEach(x => ChannelItems.Add(x));
                        break;
                }
            }
            else if (state is SyncState)
            {
                var st = (SyncState)state;
                switch (st)
                {
                    case SyncState.Added:
                        addedList.ForEach(x => ChannelItems.Add(x));
                        break;
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnStateChanged()
        {
            Title = SelectedState.State.ToString();
        }

        #endregion

        #region IChannel Members

        public CookieContainer ChannelCookies { get; set; }
        public ObservableCollection<IVideoItem> ChannelItems { get; set; }
        public ICollectionView ChannelItemsCollectionView { get; set; }
        public int ChannelItemsCount { get; set; }
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

        public void AddNewItem(IVideoItem item)
        {
            throw new NotImplementedException();
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
