using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Crawler.Common;
using Extensions;
using Interfaces.Factories;
using Interfaces.Models;
using Models.BO;
using Ninject;
//using YoutubeExtractor;

namespace Crawler.Models
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public ICommonFactory BaseFactory { get; set; }

        private string _info;

        private Cred _selectedCred;

        private Channel _selectedChannel;

        private string _newChannelName;

        private VideoItem _selectedVideoItem;

        private string _result;

        private Playlist _selectedPlaylist;

        private string _dirPath;

        private string _mpcPath;

        #region Fields

        public ObservableCollection<Channel> Channels { get; set; }

        public Channel SelectedChannel
        {
            get { return _selectedChannel; }
            set
            {
                _selectedChannel = value;
                OnPropertyChanged();
            }
        }

        public VideoItem SelectedVideoItem
        {
            get { return _selectedVideoItem; }
            set
            {
                _selectedVideoItem = value;
                OnPropertyChanged();
            }
        }

        public Playlist SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set
            {
                _selectedPlaylist = value; 
                OnPropertyChanged();
            }
        }

        public List<Cred> SupportedCreds { get; set; }

        public Cred SelectedCred
        {
            get { return _selectedCred; }
            set
            {
                _selectedCred = value;
                OnPropertyChanged();
            }
        }

        public string NewChannelName
        {
            get { return _newChannelName; }
            set
            {
                _newChannelName = value;
                OnPropertyChanged();
            }
        }

        public string NewChannelTitle { get; set; }

        public string Info
        {
            get { return _info; }
            set
            {
                _info = value;
                OnPropertyChanged();
            }
        }

        public string Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged();
            }
        }

        public string DirPath
        {
            get { return _dirPath; }
            set
            {
                _dirPath = value; 
                OnPropertyChanged();
            }
        }

        public string MpcPath
        {
            get { return _mpcPath; }
            set
            {
                _mpcPath = value; 
                OnPropertyChanged();
            }
        }

        #endregion

        public MainWindowModel()
        {
            BaseFactory = IoC.Container.Kernel.Get<ICommonFactory>();
            Channels = new ObservableCollection<Channel>();
            SupportedCreds = new List<Cred>();
            //Channels.CollectionChanged += Channels_CollectionChanged;
        }

        //void Channels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.Action == NotifyCollectionChangedAction.Remove)
        //    {
        //        foreach (var ch in e.OldItems.Cast<Channel>())
        //        {
        //            Info = "Deleted: " + ch.Title;
        //        }
        //    }

        //    if (e.Action == NotifyCollectionChangedAction.Add)
        //    {
        //        foreach (var ch in e.NewItems.Cast<Channel>())
        //        {
        //            Info = "Added: " + ch.Title;
        //        }
        //    }
        //}

        public async Task FillChannels()
        {
            var sf = BaseFactory.CreateSubscribeFactory();

            var s = sf.GetSubscribe();

            var cf = BaseFactory.CreateChannelFactory();

            try
            {
                await LoadSettings();

                var lst = await s.GetChannelsIdsListDbAsync(); //получим сначала ID каналов и начнем по одному заполнять список

                foreach (string id in lst)
                {
                    var ch = await cf.GetChannelDbAsync(id);

                    Channels.Add(ch as Channel);
                }

                //var lst = await s.GetChannelsListAsync(); //все каналы за раз - подтормаживает
                //foreach (var channel in lst)
                //{
                //    var ch = (Channel)channel;
                //    //ch.CountNew = 1;
                //    Channels.Add(ch);
                //}

                if (Channels.Any())
                    SelectedChannel = Channels.First();

                var creds = await s.GetCredListAsync();
                foreach (var cred in creds)
                {
                    var cr = (Cred)cred;
                    SupportedCreds.Add(cr);
                }

                if (SupportedCreds.Any())
                    SelectedCred = SupportedCreds.First();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //Info = Regex.Replace(ex.Message, @"\t|\n|\r", "-");
            }
        }

        public async Task SaveNewItem()
        {
            if (string.IsNullOrEmpty(NewChannelName))
            {
                MessageBox.Show("Fill user name");
                return;
            }

            Result = "Working..";

            var cf = BaseFactory.CreateChannelFactory();

            var sp = NewChannelName.Split('/');
            if (sp.Length > 1)
            {
                if (sp.Contains("user"))
                {
                    NewChannelName = await cf.GetChannelIdByUserNameNetAsync(sp.Last());
                }

                if (sp.Contains("channel"))
                    NewChannelName = sp.Last();
            }
            else
            {
                try
                {
                    NewChannelName = await cf.GetChannelIdByUserNameNetAsync(NewChannelName);
                }
                catch
                {
                    NewChannelName = NewChannelName; // :)
                }    
            }
            

            if (Channels.Select(x => x.ID).Contains(NewChannelName))
            {
                MessageBox.Show("Has already");
                return;
            }

            try
            {
                var channel = await cf.GetChannelNetAsync(NewChannelName) as Channel;
                if (channel == null)
                    throw new Exception("GetChannelNetAsync return null");

                if (!string.IsNullOrEmpty(NewChannelTitle))
                    channel.Title = NewChannelTitle;

                Channels.Add(channel);

                SelectedChannel = channel;

                var lst = await channel.GetChannelItemsNetAsync(0);

                foreach (IVideoItem item in lst)
                {
                    item.IsShowRow = true;
                    channel.ChannelItems.Add(item);
                }

                await channel.InsertChannelItemsAsync();

                var pls = await channel.GetChannelPlaylistsNetAsync();

                foreach (var pl in pls)
                {
                    channel.ChannelPlaylists.Add(pl);

                    await pl.InsertPlaylistAsync();

                    var plv = await pl.GetPlaylistItemsIdsListNetAsync();

                    foreach (string id in plv)
                    {
                        if (channel.ChannelItems.Select(x => x.ID).Contains(id))
                            await pl.UpdatePlaylistAsync(id);
                    }
                }

                Result = "Finished";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public async Task SyncData()
        {
            try
            {
                Result = "Working..";

                foreach (Channel channel in Channels)
                {
                    await channel.SyncChannelAsync(true);
                }

                //foreach (Channel channel in Channels.Where(channel => channel.CountNew > 0))
                //{
                //    foreach (IVideoItem item in channel.ChannelItems.Where(item => item.IsNewItem))
                //    {
                //        await item.InsertItemAsync();
                //    }
                //}

                Result = "Finished";
            }
            catch (Exception ex)
            {
                Result = "Error";
                MessageBox.Show(ex.Message);
                //Info = Regex.Replace(ex.Message, @"\t|\n|\r", "-");
            }
        }

        public async void SaveSettings()
        {
            var sf = BaseFactory.CreateSettingFactory();

            try
            {
                var savedir = await sf.GetSettingDbAsync("pathToDownload");

                if (savedir.Value != DirPath)
                    await savedir.UpdateSettingAsync(DirPath);

                var mpcdir = await sf.GetSettingDbAsync("pathToMpc");

                if (mpcdir.Value != MpcPath)
                    await mpcdir.UpdateSettingAsync(MpcPath);

                Result = "Saved";
            }
            catch (Exception ex)
            {
                Info = ex.Message;
            }
        }

        public async Task LoadSettings()
        {
            var sf = BaseFactory.CreateSettingFactory();

            try
            {
                var savedir = await sf.GetSettingDbAsync("pathToDownload");

                DirPath = savedir.Value;

                var mpcdir = await sf.GetSettingDbAsync("pathToMpc");

                MpcPath = mpcdir.Value;

            }
            catch (Exception ex)
            {
                Info = ex.Message;
            }
        }

        public async void DownloadItem()
        {
            if (string.IsNullOrEmpty(DirPath))
            {
                MessageBox.Show("Please, set download dir");
                return;
            }

            if (SelectedVideoItem.IsHasLocalFile == false)
            {
                //SelectedVideoItem.ProgressBarVisibility = Visibility.Visible;

                //var link = string.Format("https://www.youtube.com/watch?v={0}", SelectedVideoItem.ID);

                //var videoInfos = DownloadUrlResolver.GetDownloadUrls(link).OrderByDescending(z => z.Resolution);

                //var video = videoInfos.First(info => info.VideoType == VideoType.Mp4 && info.AudioBitrate != 0);

                //if (video.RequiresDecryption)
                //{
                //    DownloadUrlResolver.DecryptDownloadUrl(video);
                //}

                //var cltitle = video.Title.MakeValidFileName();

                //var dir = new DirectoryInfo(Path.Combine(DirPath, SelectedChannel.ID));

                //if (!dir.Exists)
                //    dir.Create();

                //var vd = new VideoDownloader(video, Path.Combine(dir.FullName, cltitle + video.VideoExtension));

                //vd.DownloadProgressChanged += (sender, args) => videoDownloader_DownloadProgressChanged(args);

                //vd.DownloadFinished += delegate { videoDownloader_DownloadFinished(vd); };

                //await Task.Run(() => vd.Execute());
            }
            else
            {
                if (!string.IsNullOrEmpty(SelectedVideoItem.LocalFilePath))
                {
                    if (!string.IsNullOrEmpty(MpcPath))
                    {
                        var param = string.Format("\"{0}\" /play", SelectedVideoItem.LocalFilePath);
                        Process.Start(MpcPath, param);
                    }
                }
            }
        }

        //private void videoDownloader_DownloadFinished(VideoDownloader vd)
        //{
        //    if (vd == null) return;

        //    Info = string.Format("\"{0}\" completed!", vd.Video.Title);

        //    SelectedVideoItem.LocalFilePath = vd.SavePath;

        //    SelectedVideoItem.IsHasLocalFile = true;

        //    //SelectedVideoItem.ProgressBarVisibility = Visibility.Hidden;
        //}

        //private void videoDownloader_DownloadProgressChanged(ProgressEventArgs args)
        //{
        //    SelectedVideoItem.DownloadPercentage = args.ProgressPercentage;
        //}
    }
}
