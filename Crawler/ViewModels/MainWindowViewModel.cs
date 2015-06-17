using System.Windows;
using System.Windows.Forms;
using Crawler.Common;
using Crawler.Models;
using Crawler.Views;
using Application = System.Windows.Application;

namespace Crawler.ViewModels
{
    public class MainWindowViewModel
    {
        public MainWindowModel Model { get; set; }

        public RelayCommand AddNewItemCommand { get; set; }

        public RelayCommand SaveNewItemCommand { get; set; }

        public RelayCommand SyncDataCommand { get; set; }

        public RelayCommand OpenDirCommand { get; set; }

        public RelayCommand SaveCommand { get; set; }

        public RelayCommand DownloadLinkCommand { get; set; }

        public MainWindowViewModel(MainWindowModel model)
        {
            Model = model;
            OpenDirCommand = new RelayCommand(OpenDir);
            AddNewItemCommand = new RelayCommand(x => AddNewItem(false));
            SaveNewItemCommand = new RelayCommand(async x => await Model.SaveNewItem());
            SyncDataCommand = new RelayCommand(async x => await Model.SyncData());
            SaveCommand = new RelayCommand(async x => await Model.SaveSettings());
            DownloadLinkCommand = new RelayCommand(async x => await Model.DownloadLink());
        }

        private void OpenDir(object obj)
        {
            switch (obj.ToString())
            {
                case "DirPath":
                    var dlg = new FolderBrowserDialog();
                    var res = dlg.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        Model.DirPath = dlg.SelectedPath;
                    }
                    break;

                case "MpcPath":
                    var dlgm = new OpenFileDialog {Filter = @"EXE files (*.exe)|*.exe"};
                    var resm = dlgm.ShowDialog();
                    if (resm == DialogResult.OK)
                    {
                        Model.MpcPath = dlgm.FileName;
                    }
                    break;

                case "YouPath":
                    var dlgy = new OpenFileDialog {Filter = @"EXE files (*.exe)|*.exe"};
                    var resy = dlgy.ShowDialog();
                    if (resy == DialogResult.OK)
                    {
                        Model.YouPath = dlgy.FileName;
                    }
                    break;
            }
        }

        public void AddNewItem(bool isEditMode)
        {
            Model.IsEditMode = isEditMode;

            var addview = new AddChanelView
            {
                DataContext = this,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            addview.ShowDialog();
        }

        public void OpenSettings()
        {
            var set = new SettingsView
            {
                DataContext = this,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            set.ShowDialog();
        }

        public void OpenAddLink()
        {
            var adl = new AddLinkView
            {
                DataContext = this,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            adl.ShowDialog();
        }
    }
}
