// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Crawler.Common;
using Crawler.Properties;
using Crawler.Views;
using DataAPI.POCO;
using Extensions;
using Extensions.Helpers;
using Interfaces.Enums;
using Interfaces.Models;
using Models.Factories;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Crawler.ViewModels
{
    public sealed class SettingsViewModel : INotifyPropertyChanged
    {
        #region Constants

        private const string dirLaunchParam = "dir";
        private const string exeFilter = "EXE files (*.exe)|*.exe";
        private const string mpcLaunchParam = "mpc";
        private const string pathToDownload = "pathToDownload";
        private const string pathToMpc = "pathToMpc";
        private const string pathToYoudl = "pathToYoudl";
        private const string youLaunchParam = "you";
        private const string youheader = "Youtube-dl";
        private const string youtubeDl = "youtube-dl.exe";
        private readonly Action<string> onSaveAction;

        #endregion

        #region Fields

        private RelayCommand addNewTagCommand;
        private RelayCommand deleteTagCommand;
        private string dirPath;
        private RelayCommand fillYouHeaderCommand;
        private bool isUpdateButtonEnable = true;
        private string mpcPath;
        private RelayCommand openDirCommand;
        private double prValue;
        private RelayCommand saveSettingsCommand;
        private RelayCommand updateYouDlCommand;
        private string youHeader;
        private string youPath;

        #endregion

        #region Constructors

        public SettingsViewModel(Action<string> onSaveAction)
        {
            this.onSaveAction = onSaveAction;
            SupportedTags = new ObservableCollection<ITag>();
            SupportedCreds = new List<ICred>();
        }

        #endregion

        #region Properties

        public RelayCommand AddNewTagCommand
        {
            get
            {
                return addNewTagCommand ?? (addNewTagCommand = new RelayCommand(x => AddNewTag()));
            }
        }

        public RelayCommand DeleteTagCommand
        {
            get
            {
                return deleteTagCommand ?? (deleteTagCommand = new RelayCommand(DeleteTag));
            }
        }

        public string DirPath
        {
            get
            {
                return dirPath;
            }
            set
            {
                dirPath = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand FillYouHeaderCommand
        {
            get
            {
                return fillYouHeaderCommand ?? (fillYouHeaderCommand = new RelayCommand(x => FillYouHeader()));
            }
        }

        public bool IsUpdateButtonEnable
        {
            get
            {
                return isUpdateButtonEnable;
            }
            set
            {
                isUpdateButtonEnable = value;
                OnPropertyChanged();
            }
        }

        public string MpcPath
        {
            get
            {
                return mpcPath;
            }
            set
            {
                mpcPath = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand OpenDirCommand
        {
            get
            {
                return openDirCommand ?? (openDirCommand = new RelayCommand(OpenDir));
            }
        }

        public double PrValue
        {
            get
            {
                return prValue;
            }
            private set
            {
                prValue = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand SaveSettingsCommand
        {
            get
            {
                return saveSettingsCommand ?? (saveSettingsCommand = new RelayCommand(async x => await SaveSettingsToDb()));
            }
        }

        public List<ICred> SupportedCreds { get; set; }
        public ObservableCollection<ITag> SupportedTags { get; set; }

        public RelayCommand UpdateYouDlCommand
        {
            get
            {
                return updateYouDlCommand ?? (updateYouDlCommand = new RelayCommand(x => UpdateYouDl()));
            }
        }

        public string YouHeader
        {
            get
            {
                return youHeader;
            }
            private set
            {
                youHeader = value;
                OnPropertyChanged();
            }
        }

        public string YouPath
        {
            get
            {
                return youPath;
            }
            set
            {
                youPath = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Static Methods

        public static bool IsFfmegExist()
        {
            const string ff = "ffmpeg.exe";
            string values = Environment.GetEnvironmentVariable("PATH");
            return values != null && values.Split(';').Select(path => Path.Combine(path, ff)).Any(File.Exists);
        }

        #endregion

        #region Methods

        public bool IsMpcExist()
        {
            if (!string.IsNullOrEmpty(MpcPath))
            {
                return true;
            }
            MessageBox.Show("Please, select MPC");
            return false;
        }

        public async Task LoadCredsFromDb()
        {
            IEnumerable<CredPOCO> fbres = await CommonFactory.CreateSqLiteDatabase().GetCredListAsync();
            SupportedCreds.AddRange(fbres.Select(CredFactory.CreateCred));
        }

        public bool IsYoutubeExist()
        {
            const string mess = "Please, select youtube-dl";
            if (!string.IsNullOrEmpty(YouPath))
            {
                var fn = new FileInfo(YouPath);
                if (fn.Exists)
                {
                    return true;
                }
                MessageBox.Show(mess);
                return false;
            }
            MessageBox.Show(mess);
            return false;
        }

        public async Task LoadSettingsFromDb()
        {
            SettingFactory sf = CommonFactory.CreateSettingFactory();

            ISetting savedir = await SettingFactory.GetSettingDbAsync(pathToDownload);
            DirPath = savedir.Value;
            if (string.IsNullOrEmpty(DirPath))
            {
                DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            ISetting mpcdir = await SettingFactory.GetSettingDbAsync(pathToMpc);
            MpcPath = mpcdir.Value;

            ISetting youpath = await SettingFactory.GetSettingDbAsync(pathToYoudl);
            YouPath = youpath.Value;

            if (string.IsNullOrEmpty(YouPath))
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string res = Path.Combine(path, youtubeDl);
                var fn = new FileInfo(res);
                if (fn.Exists)
                {
                    YouPath = fn.FullName;
                }
            }
        }

        public void LoadSettingsFromLaunchParam(IReadOnlyDictionary<string, string> launchParams)
        {
            string param;
            if (launchParams.TryGetValue(dirLaunchParam, out param))
            {
                var di = new DirectoryInfo(param);
                DirPath = di.Exists ? di.FullName : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            if (launchParams.TryGetValue(mpcLaunchParam, out param))
            {
                var fn = new FileInfo(param);
                if (fn.Exists)
                {
                    MpcPath = fn.FullName;
                }
            }

            if (launchParams.TryGetValue(youLaunchParam, out param))
            {
                var fn = new FileInfo(param);
                if (fn.Exists)
                {
                    YouPath = fn.FullName;
                }
            }
        }

        public async Task LoadTagsFromDb()
        {
            IEnumerable<TagPOCO> fbres = (await CommonFactory.CreateSqLiteDatabase().GetAllTagsAsync()).ToArray();
            IEnumerable<ITag> lst = fbres.Select(TagFactory.CreateTag);
            foreach (ITag tag in lst)
            {
                SupportedTags.Add(tag);
            }
        }

        private void AddNewTag()
        {
            var advm = new AddNewTagViewModel(this) { Tag = TagFactory.CreateTag() };
            var antv = new AddNewTagView
            {
                DataContext = advm,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            antv.ShowDialog();
        }

        private async void DeleteTag(object obj)
        {
            var tag = obj as ITag;
            if (tag == null)
            {
                return;
            }
            MessageBoxResult result =
                MessageBox.Show(string.Format("Are you sure to delete Tag:{0}[{1}]" + "?", Environment.NewLine, tag.Title),
                    "Confirm",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Information);

            if (result != MessageBoxResult.OK)
            {
                return;
            }
            SupportedTags.Remove(tag);
            await tag.DeleteTagAsync();
        }

        private void FillYouHeader()
        {
            YouHeader = string.IsNullOrEmpty(YouPath)
                ? youheader
                : string.Format("{0} ({1})", youheader, CommonExtensions.GetConsoleOutput(YouPath, "--version", true).Trim());
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OpenDir(object obj)
        {
            var item = (OpenDirParam)obj;
            switch (item)
            {
                case OpenDirParam.DirPath:

                    var dlg = new FolderBrowserDialog();
                    DialogResult res = dlg.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        DirPath = dlg.SelectedPath;
                    }

                    break;
                case OpenDirParam.MpcPath:

                    var dlgm = new OpenFileDialog { Filter = exeFilter };
                    DialogResult resm = dlgm.ShowDialog();
                    if (resm == DialogResult.OK)
                    {
                        MpcPath = dlgm.FileName;
                    }

                    break;
                case OpenDirParam.YouPath:

                    var dlgy = new OpenFileDialog { Filter = exeFilter };
                    DialogResult resy = dlgy.ShowDialog();
                    if (resy == DialogResult.OK)
                    {
                        YouPath = dlgy.FileName;
                        FillYouHeader();
                    }

                    break;
            }
        }

        private async Task SaveSettingsToDb()
        {
            ISetting savedir = await SettingFactory.GetSettingDbAsync(pathToDownload);
            if (savedir.Value != DirPath)
            {
                await savedir.UpdateSettingAsync(DirPath);
                if (onSaveAction != null)
                {
                    onSaveAction.Invoke(DirPath);
                }
            }

            ISetting mpcdir = await SettingFactory.GetSettingDbAsync(pathToMpc);
            if (mpcdir.Value != MpcPath)
            {
                await mpcdir.UpdateSettingAsync(MpcPath);
            }

            ISetting youpath = await SettingFactory.GetSettingDbAsync(pathToYoudl);
            if (youpath.Value != YouPath)
            {
                await youpath.UpdateSettingAsync(YouPath);
            }

            foreach (ICred cred in SupportedCreds)
            {
                await cred.UpdateLoginAsync(cred.Login);
                await cred.UpdatePasswordAsync(cred.Pass);
            }

            foreach (ITag tag in SupportedTags)
            {
                await tag.InsertTagAsync();
            }

            if (onSaveAction != null)
            {
                onSaveAction.Invoke(null);
            }
        }

        private void UpdateYouDl()
        {
            var link = (string)Settings.Default["pathToYoudl"];

            if (string.IsNullOrEmpty(YouPath))
            {
                YouPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), link.Split('/').Last());
            }

            if (SiteHelper.CheckForInternetConnection(link))
            {
                IsUpdateButtonEnable = false;
                YouHeader = "Youtube-dl (update in progress..)";

                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += ClientDownloadProgressChanged;
                    client.DownloadFileCompleted += ClientDownloadFileCompleted;
                    client.DownloadFileAsync(new Uri(link), YouPath);
                }
            }
            else
            {
                MessageBox.Show(link + " is not available");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Event Handling

        private void ClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            IsUpdateButtonEnable = true;
            PrValue = 0;
            YouHeader = string.Format("{0} ({1})", youheader, CommonExtensions.GetConsoleOutput(YouPath, "--version", true).Trim());

            var webClient = sender as WebClient;
            if (webClient == null)
            {
                return;
            }
            webClient.DownloadFileCompleted -= ClientDownloadFileCompleted;
            webClient.DownloadProgressChanged -= ClientDownloadProgressChanged;
        }

        private void ClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString(CultureInfo.InvariantCulture));
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString(CultureInfo.InvariantCulture));
            PrValue = bytesIn / totalBytes * 100;
        }

        #endregion
    }
}
