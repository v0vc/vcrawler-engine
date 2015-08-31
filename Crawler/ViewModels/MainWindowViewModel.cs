using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Crawler.Common;
using Crawler.Models;
using Crawler.Views;
using Microsoft.WindowsAPICodePack.Taskbar;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Crawler.ViewModels
{
    public class MainWindowViewModel
    {
        public MainWindowViewModel(MainWindowModel model)
        {
            Model = model;
            OpenDirCommand = new RelayCommand(OpenDir);
            AddNewItemCommand = new RelayCommand(x => AddNewItem(false));
            AddNewTagCommand = new RelayCommand(x => AddNewTag());
            SaveNewItemCommand = new RelayCommand(async x => await Model.SaveNewItem());
            SyncDataCommand = new RelayCommand(async x => await Model.SyncData());
            SaveCommand = new RelayCommand(async x => await Model.SaveSettings());
            DownloadLinkCommand = new RelayCommand(async x => await Model.DownloadLink());
            SearchCommand = new RelayCommand(async x => await Model.Search());
        }

        public MainWindowModel Model { get; set; }
        public RelayCommand AddNewItemCommand { get; set; }
        public RelayCommand AddNewTagCommand { get; set; }
        public RelayCommand SaveNewItemCommand { get; set; }
        public RelayCommand SyncDataCommand { get; set; }
        public RelayCommand OpenDirCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand DownloadLinkCommand { get; set; }
        public RelayCommand SearchCommand { get; set; }

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
                    var dlgm = new OpenFileDialog { Filter = @"EXE files (*.exe)|*.exe" };
                    var resm = dlgm.ShowDialog();
                    if (resm == DialogResult.OK)
                    {
                        Model.MpcPath = dlgm.FileName;
                    }
                    break;

                case "YouPath":
                    var dlgy = new OpenFileDialog { Filter = @"EXE files (*.exe)|*.exe" };
                    var resy = dlgy.ShowDialog();
                    if (resy == DialogResult.OK)
                    {
                        Model.YouPath = dlgy.FileName;
                    }
                    break;
            }
        }

        private void AddNewTag()
        {
            var antv = new AddNewTagView
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                DataContext = this
            };

            antv.ShowDialog();
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

        public async Task Backup()
        {
            var dlg = new SaveFileDialog
            {
                FileName = "backup_" + DateTime.Now.ToShortDateString(), 
                DefaultExt = ".txt", 
                Filter = @"Text documents (.txt)|*.txt", 
                OverwritePrompt = true
            };
            var res = dlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                var fb = Model.BaseFactory.CreateSqLiteDatabase();
                var lst = (await fb.GetChannelsListAsync()).ToList();
                var sb = new StringBuilder();
                foreach (var poco in lst)
                {
                    sb.Append(poco.Title)
                        .Append("|")
                        .Append(poco.ID)
                        .Append("|")
                        .Append(poco.Site)
                        .Append(Environment.NewLine);
                }
                try
                {
                    File.WriteAllText(dlg.FileName, sb.ToString().TrimEnd('\r', '\n'));
                    Model.Info = string.Format("{0} channels has been stored", lst.Count);
                    Model.SetStatus(2);
                }
                catch (Exception ex)
                {
                    Model.Info = ex.Message;
                    Model.SetStatus(3);
                }
            }
        }

        public async Task Restore()
        {
            Model.Info = string.Empty;

            var opf = new OpenFileDialog { Filter = @"Text documents (.txt)|*.txt" };
            var res = opf.ShowDialog();

            if (res == DialogResult.OK)
            {
                string[] lst = { };

                try
                {
                    lst = File.ReadAllLines(opf.FileName);
                }
                catch (Exception ex)
                {
                    Model.Info = ex.Message;
                    Model.SetStatus(3);
                }

                Model.SetStatus(1);
                var prog = TaskbarManager.Instance;
                prog.SetProgressState(TaskbarProgressBarState.Normal);
                Model.ShowAllChannels();
                Model.IsIdle = false;
                var rest = 0;
                foreach (var s in lst)
                {
                    var sp = s.Split('|');
                    if (sp.Length == 3)
                    {
                        if (Model.Channels.Select(x => x.ID).Contains(sp[1]))
                        {
                            continue;
                        }

                        switch (sp[2])
                        {
                            case "youtube.com":

                                try
                                {
                                    Model.SetStatus(1);
                                    Model.Info = "Restoring: " + sp[0];
                                    await Model.AddNewChannelAsync(sp[1], null);
                                }
                                catch (Exception ex)
                                {
                                    Model.SetStatus(3);
                                    Model.Info = "Can't restore: " + sp[0];
                                    MessageBox.Show(ex.Message);
                                }

                                rest++;
                                Model.PrValue = Math.Round((double)(100 * rest) / lst.Count());
                                prog.SetProgressValue((int)Model.PrValue, 100);
                                break;

                            default:

                                Model.Info = "Unsupported site: " + sp[2];

                                break;
                        }
                    }
                    else
                    {
                        Model.Info = "Check: " + s;
                    }
                }

                prog.SetProgressState(TaskbarProgressBarState.NoProgress);
                Model.PrValue = 0;
                Model.IsIdle = true;
                Model.SetStatus(2);

                Model.Info = "Total restored: " + rest;
            }
        }

        public async Task Vacuumdb()
        {
            var db = Model.BaseFactory.CreateSqLiteDatabase();
            var sizebefore = db.FileBase.Length;
            await db.VacuumAsync();
            var sizeafter = new FileInfo(db.FileBase.FullName).Length;
            Model.Info = string.Format("Database compacted (bytes): {0} -> {1}", sizebefore, sizeafter);
        }
    }
}
