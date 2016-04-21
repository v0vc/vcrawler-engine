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
using DataAPI.Database;
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
        private const string onstartupopen = "youtubeBest";
        private const string pathToDownload = "pathToDownload";
        private const string pathToMpc = "pathToMpc";
        private const string pathToYoudl = "pathToYoudl";
        private const string youheader = "Youtube-dl";
        private const string youLaunchParam = "you";
        private const string youtubeDl = "youtube-dl.exe";

        #endregion

        #region Static and Readonly Fields

        private readonly SqLiteDatabase db;
        private readonly Action<string> onSaveAction;

        #endregion

        #region Fields

        private RelayCommand addNewTagCommand;
        private RelayCommand deleteTagCommand;
        private string dirPath;
        private RelayCommand fillYouHeaderCommand;
        private bool isFilterOpen;
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

        public SettingsViewModel(SqLiteDatabase db, Action<string> onSaveAction)
        {
            this.db = db;
            this.onSaveAction = onSaveAction;
            SupportedTags = new ObservableCollection<ITag>();
            SupportedCreds = new List<ICred>();
        }

        #endregion

        #region Properties

        public RelayCommand AddNewTagCommand => addNewTagCommand ?? (addNewTagCommand = new RelayCommand(x => AddNewTag()));

        public RelayCommand DeleteTagCommand => deleteTagCommand ?? (deleteTagCommand = new RelayCommand(DeleteTag));

        public string DirPath
        {
            get
            {
                return dirPath;
            }
            set
            {
                if (value == dirPath)
                {
                    return;
                }
                dirPath = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand FillYouHeaderCommand
            => fillYouHeaderCommand ?? (fillYouHeaderCommand = new RelayCommand(x => FillYouHeader()));

        public bool IsFilterOpen
        {
            get
            {
                return isFilterOpen;
            }
            set
            {
                if (value == isFilterOpen)
                {
                    return;
                }
                isFilterOpen = value;
                OnPropertyChanged();
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
                if (value == isUpdateButtonEnable)
                {
                    return;
                }
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
                if (value == mpcPath)
                {
                    return;
                }
                mpcPath = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand OpenDirCommand => openDirCommand ?? (openDirCommand = new RelayCommand(OpenDir));

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
            => saveSettingsCommand ?? (saveSettingsCommand = new RelayCommand(async x => await SaveSettingsToDb().ConfigureAwait(false)));

        public List<ICred> SupportedCreds { get; set; }
        public ObservableCollection<ITag> SupportedTags { get; set; }

        public RelayCommand UpdateYouDlCommand => updateYouDlCommand ?? (updateYouDlCommand = new RelayCommand(x => UpdateYouDl()));

        public string YouHeader
        {
            get
            {
                return youHeader;
            }
            private set
            {
                if (value == youHeader)
                {
                    return;
                }
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
                if (value == youPath)
                {
                    return;
                }
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

        public async Task LoadCredsFromDb()
        {
            List<CredPOCO> fbres = await db.GetCredListAsync().ConfigureAwait(false);
            SupportedCreds.AddRange(fbres.Select(CredFactory.CreateCred));
        }

        public async Task LoadSettingsFromDb()
        {
            ISetting savedir = await SettingFactory.GetSettingDbAsync(pathToDownload).ConfigureAwait(false);
            DirPath = savedir.Value;
            if (string.IsNullOrEmpty(DirPath))
            {
                DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            ISetting mpcdir = await SettingFactory.GetSettingDbAsync(pathToMpc).ConfigureAwait(false);
            MpcPath = mpcdir.Value;

            ISetting youpath = await SettingFactory.GetSettingDbAsync(pathToYoudl).ConfigureAwait(false);
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

            ISetting onstartup = await SettingFactory.GetSettingDbAsync(onstartupopen).ConfigureAwait(false);
            IsFilterOpen = onstartup.Value != "0";
        }

        public async Task LoadSettingsFromLaunchParam(IReadOnlyDictionary<string, string> launchParams)
        {
            string param;
            if (launchParams.TryGetValue(dirLaunchParam, out param))
            {
                var di = new DirectoryInfo(param);
                DirPath = di.Exists ? di.FullName : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                ISetting savedir = await SettingFactory.GetSettingDbAsync(pathToDownload).ConfigureAwait(false);
                DirPath = savedir.Value;
                if (string.IsNullOrEmpty(DirPath))
                {
                    DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
            }

            if (launchParams.TryGetValue(mpcLaunchParam, out param))
            {
                var fn = new FileInfo(param);
                if (fn.Exists)
                {
                    MpcPath = fn.FullName;
                }
            }
            else
            {
                ISetting mpcdir = await SettingFactory.GetSettingDbAsync(pathToMpc).ConfigureAwait(false);
                MpcPath = mpcdir.Value;
            }

            if (launchParams.TryGetValue(youLaunchParam, out param))
            {
                var fn = new FileInfo(param);
                if (fn.Exists)
                {
                    YouPath = fn.FullName;
                }
            }
            else
            {
                ISetting youpath = await SettingFactory.GetSettingDbAsync(pathToYoudl).ConfigureAwait(false);
                YouPath = youpath.Value;
            }

            ISetting onstartup = await SettingFactory.GetSettingDbAsync(onstartupopen).ConfigureAwait(false);
            IsFilterOpen = onstartup.Value != "0";
        }

        public async Task LoadTagsFromDb()
        {
            List<TagPOCO> fbres = await db.GetAllTagsAsync().ConfigureAwait(false);
            IEnumerable<ITag> lst = fbres.Select(TagFactory.CreateTag);
            foreach (ITag tag in lst)
            {
                SupportedTags.Add(tag);
            }
        }

        private void AddNewTag()
        {
            var advm = new AddTagViewModel(true, null, SupportedTags);
            var antv = new AddTagView
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
            MessageBoxResult result = MessageBox.Show($"Are you sure to delete Tag:{Environment.NewLine}[{tag.Title}]" + "?",
                "Confirm",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information);

            if (result != MessageBoxResult.OK)
            {
                return;
            }
            SupportedTags.Remove(tag);
            await db.DeleteTagAsync(tag.Title).ConfigureAwait(false);
        }

        private void FillYouHeader()
        {
            YouHeader = string.IsNullOrEmpty(YouPath)
                ? youheader
                : $"{youheader} ({CommonExtensions.GetConsoleOutput(YouPath, "--version", true).Trim()})";
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            ISetting savedir = await SettingFactory.GetSettingDbAsync(pathToDownload).ConfigureAwait(false);
            if (savedir.Value != DirPath)
            {
                await db.UpdateSettingAsync(savedir.Key, DirPath).ConfigureAwait(false);
                onSaveAction?.Invoke(DirPath);
            }

            ISetting mpcdir = await SettingFactory.GetSettingDbAsync(pathToMpc).ConfigureAwait(false);
            if (mpcdir.Value != MpcPath)
            {
                await db.UpdateSettingAsync(mpcdir.Key, MpcPath).ConfigureAwait(false);
            }

            ISetting youpath = await SettingFactory.GetSettingDbAsync(pathToYoudl).ConfigureAwait(false);
            if (youpath.Value != YouPath)
            {
                await db.UpdateSettingAsync(youpath.Key, YouPath).ConfigureAwait(false);
            }

            ISetting onstartup = await SettingFactory.GetSettingDbAsync(onstartupopen).ConfigureAwait(false);
            string res = IsFilterOpen ? "1" : "0";
            if (onstartup.Value != res)
            {
                await db.UpdateSettingAsync(onstartup.Key, res).ConfigureAwait(false);
            }

            foreach (ICred cred in SupportedCreds)
            {
                await db.UpdateLoginAsync(cred.SiteAdress, cred.Login).ConfigureAwait(false);
                await db.UpdatePasswordAsync(cred.SiteAdress, cred.Pass).ConfigureAwait(false);
            }

            foreach (ITag tag in SupportedTags)
            {
                await db.InsertTagAsync(tag).ConfigureAwait(false);
            }

            onSaveAction?.Invoke(null);
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
            YouHeader = $"{youheader} ({CommonExtensions.GetConsoleOutput(YouPath, "--version", true).Trim()})";

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
