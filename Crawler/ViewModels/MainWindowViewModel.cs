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

        public MainWindowViewModel(MainWindowModel model)
        {
            Model = model;
            AddNewItemCommand = new RelayCommand(x => AddNewItem());
            SaveNewItemCommand = new RelayCommand(x => Model.SaveNewItem());
            SyncDataCommand = new RelayCommand(x => Model.SyncData());
            OpenDirCommand = new RelayCommand(OpenDir);
            SaveCommand = new RelayCommand(x => Model.SaveSettings());
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

                //case "FfmegPath":
                //    var dlgf = new OpenFileDialog {Filter = @"EXE files (*.exe)|*.exe"};
                //    var resf = dlgf.ShowDialog();
                //    if (resf == DialogResult.OK)
                //    {
                //        Model.FfmegPath = dlgf.FileName;
                //    }
                //    break;
            }
        }

        private void AddNewItem()
        {
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
    }
}
