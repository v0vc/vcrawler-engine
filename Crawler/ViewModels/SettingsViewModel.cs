// This file contains my intellectual property. Release of this file requires prior approval from me.
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
using Extensions;
using Interfaces.Enums;
using Interfaces.Factories;
using Interfaces.Models;
using Interfaces.POCO;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Crawler.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        #region Constants

        private const string dirLaunchParam = "dir";
        private const string exeFilter = "EXE files (*.exe)|*.exe";
        private const string mpcLaunchParam = "mpc";
        private const string pathToDownload = "pathToDownload";
        private const string pathToMpc = "pathToMpc";
        private const string pathToYoudl = "pathToYoudl";
        private const string youheader = "Youtube-dl";
        private const string youLaunchParam = "you";
        private const string youtubeDl = "youtube-dl.exe";

        #endregion

        #region Static and Readonly Fields

        private readonly ICommonFactory baseFactory;

        #endregion

        #region Fields

        private RelayCommand addNewTagCommand;
        private RelayCommand deleteTagCommand;
        private string dirPath;
        private RelayCommand fillYouHeaderCommand;
        private string mpcPath;
        private RelayCommand openDirCommand;
        private RelayCommand saveSettingsCommand;
        private RelayCommand updateYouDlCommand;
        private string youHeader;
        private string youPath;

        #endregion

        #region Constructors

        public SettingsViewModel(ICommonFactory baseFactory)
        {
            this.baseFactory = baseFactory;
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

        private static bool CheckForInternetConnection(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (Stream stream = client.OpenRead(url))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Methods

        public async Task LoadCredsFromDb()
        {
            IEnumerable<ICredPOCO> fbres = await baseFactory.CreateSqLiteDatabase().GetCredListAsync();
            SupportedCreds.AddRange(fbres.Select(poco => baseFactory.CreateCredFactory().CreateCred(poco)));
        }

        public async Task LoadSettingsFromDb()
        {
            ISettingFactory sf = baseFactory.CreateSettingFactory();

            ISetting savedir = await sf.GetSettingDbAsync(pathToDownload);
            DirPath = savedir.Value;
            if (string.IsNullOrEmpty(DirPath))
            {
                DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            ISetting mpcdir = await sf.GetSettingDbAsync(pathToMpc);
            MpcPath = mpcdir.Value;

            ISetting youpath = await sf.GetSettingDbAsync(pathToYoudl);
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
            IEnumerable<ITagPOCO> fbres = (await baseFactory.CreateSqLiteDatabase().GetAllTagsAsync()).ToArray();
            IEnumerable<ITag> lst = fbres.Select(poco => baseFactory.CreateTagFactory().CreateTag(poco));
            foreach (ITag tag in lst)
            {
                SupportedTags.Add(tag);
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

        private void AddNewTag()
        {
            var advm = new AddNewTagViewModel(this) { Tag = baseFactory.CreateTagFactory().CreateTag() };
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
                    }

                    break;
            }
        }

        private async Task SaveSettingsToDb()
        {
            ISettingFactory sf = baseFactory.CreateSettingFactory();

            ISetting savedir = await sf.GetSettingDbAsync(pathToDownload);
            if (savedir.Value != DirPath)
            {
                await savedir.UpdateSettingAsync(DirPath);
            }

            ISetting mpcdir = await sf.GetSettingDbAsync(pathToMpc);
            if (mpcdir.Value != MpcPath)
            {
                await mpcdir.UpdateSettingAsync(MpcPath);
            }

            ISetting youpath = await sf.GetSettingDbAsync(pathToYoudl);
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
        }

        private void UpdateYouDl()
        {
            var link = (string)Settings.Default["pathToYoudl"];

            if (string.IsNullOrEmpty(YouPath))
            {
                YouPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), link.Split('/').Last());
            }

            // ViewModel.Model.IsWorking = true;

            // ViewModel.Model.Info = CommonExtensions.GetConsoleOutput(ViewModel.Model.YouPath, "--rm-cache-dir", false);
            if (CheckForInternetConnection(link))
            {
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
                // ViewModel.Model.IsWorking = false;
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
            YouHeader = string.Format("{0} ({1})", youheader, CommonExtensions.GetConsoleOutput(YouPath, "--version", true).Trim());

            // ViewModel.Model.PrValue = 0;
            // ViewModel.Model.IsWorking = false;
            // ViewModel.Model.Info = e.Error == null ? "Youtube-dl has been updated" : e.Error.InnerException.Message;
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

            // ViewModel.Model.PrValue = bytesIn / totalBytes * 100;
        }

        #endregion
    }
}
