// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Crawler.Common;
using Crawler.Models;
using Crawler.Views;
using Interfaces.API;
using Interfaces.Enums;
using Interfaces.POCO;
using Microsoft.WindowsAPICodePack.Taskbar;
using Application = System.Windows.Application;
using DataGrid = System.Windows.Controls.DataGrid;
using MessageBox = System.Windows.MessageBox;

namespace Crawler.ViewModels
{
    public class MainWindowViewModel
    {
        #region Fields

        private RelayCommand addNewItemCommand;
        private RelayCommand addNewTagCommand;
        private RelayCommand channelSelectionChangedCommand;
        private RelayCommand downloadLinkCommand;
        private RelayCommand fillChannelsCommand;
        private RelayCommand openDirCommand;
        private RelayCommand saveCommand;
        private RelayCommand saveNewItemCommand;
        private RelayCommand searchCommand;
        private RelayCommand syncDataCommand;
        private RelayCommand mainMenuCommand;

        #endregion

        #region Constructors

        public MainWindowViewModel(MainWindowModel model)
        {
            Model = model;
        }

        #endregion

        #region Properties

        public RelayCommand MainMenuCommand
        {
            get
            {
                return mainMenuCommand ?? (mainMenuCommand = new RelayCommand(MainMenuClick));
            }
        }

        private async void MainMenuClick(object param)
        {
            var par = param as string;
            if (!string.IsNullOrEmpty(par))
            {
                switch (par)
                {
                    case "Backup":

                        await Backup();

                        break;

                    case "Restore":

                        await Restore();

                        break;

                    case "Settings":

                        OpenSettings();

                        break;

                    case "Vacuum":

                        await Vacuumdb();

                        break;

                    case "ShowAll":

                        Model.ShowAllChannels();

                        break;

                    case "Link":

                        OpenAddLink();

                        break;

                    case "About":

                        MessageBox.Show("by v0v © 2015", "About", MessageBoxButton.OK, MessageBoxImage.Information);

                        break;
                }
            }
            else
            {
                var window = param as Window;
                if (window != null)
                {
                    window.Close();
                }
            }
        }

        public RelayCommand AddNewItemCommand
        {
            get
            {
                return addNewItemCommand ?? (addNewItemCommand = new RelayCommand(x => AddNewItem(false)));
            }
        }

        public RelayCommand AddNewTagCommand
        {
            get
            {
                return addNewTagCommand ?? (addNewTagCommand = new RelayCommand(x => AddNewTag()));
            }
        }

        public RelayCommand ChannelSelectionChangedCommand
        {
            get
            {
                return channelSelectionChangedCommand ?? (channelSelectionChangedCommand = new RelayCommand(FocusRow));
            }
        }

        public RelayCommand DownloadLinkCommand
        {
            get
            {
                return downloadLinkCommand ?? (downloadLinkCommand = new RelayCommand(async x => await Model.DownloadLink()));
            }
        }

        public RelayCommand FillChannelsCommand
        {
            get
            {
                return fillChannelsCommand ?? (fillChannelsCommand = new RelayCommand(x => Model.OnStartup()));
            }
        }

        public MainWindowModel Model { get; set; }

        public RelayCommand OpenDirCommand
        {
            get
            {
                return openDirCommand ?? (openDirCommand = new RelayCommand(OpenDir));
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ?? (saveCommand = new RelayCommand(async x => await Model.SaveSettings()));
            }
        }

        public RelayCommand SaveNewItemCommand
        {
            get
            {
                return saveNewItemCommand ?? (saveNewItemCommand = new RelayCommand(async x => await Model.SaveNewItem()));
            }
        }

        public RelayCommand SearchCommand
        {
            get
            {
                return searchCommand ?? (searchCommand = new RelayCommand(async x => await Model.Search()));
            }
        }

        public RelayCommand SyncDataCommand
        {
            get
            {
                return syncDataCommand ?? (syncDataCommand = new RelayCommand(async x => await Model.SyncData()));
            }
        }

        #endregion

        #region Methods

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

        public async Task Backup()
        {
            var dlg = new SaveFileDialog
            {
                FileName = "backup_" + DateTime.Now.ToShortDateString(),
                DefaultExt = ".txt",
                Filter = @"Text documents (.txt)|*.txt",
                OverwritePrompt = true
            };
            DialogResult res = dlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                ISqLiteDatabase fb = Model.BaseFactory.CreateSqLiteDatabase();
                List<IChannelPOCO> lst = (await fb.GetChannelsListAsync()).ToList();
                var sb = new StringBuilder();
                foreach (IChannelPOCO poco in lst)
                {
                    sb.Append(poco.Title).Append("|").Append(poco.ID).Append("|").Append(poco.Site).Append(Environment.NewLine);
                }
                try
                {
                    File.WriteAllText(dlg.FileName, sb.ToString().TrimEnd('\r', '\n'));
                    Model.Info = string.Format("{0} channels has been stored", lst.Count);
                    Model.SetStatus(0);
                }
                catch (Exception ex)
                {
                    Model.Info = ex.Message;
                    Model.SetStatus(3);
                }
            }
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

        public async Task Restore()
        {
            Model.Info = string.Empty;

            var opf = new OpenFileDialog { Filter = @"Text documents (.txt)|*.txt" };
            DialogResult res = opf.ShowDialog();

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
                TaskbarManager prog = TaskbarManager.Instance;
                prog.SetProgressState(TaskbarProgressBarState.Normal);
                Model.ShowAllChannels();
                int rest = 0;
                foreach (string s in lst)
                {
                    string[] sp = s.Split('|');
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
                                    await Model.AddNewChannelAsync(sp[1], null, SiteType.YouTube);
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
                Model.SetStatus(0);
                Model.Info = "Total restored: " + rest;
            }
        }

        public async Task Vacuumdb()
        {
            ISqLiteDatabase db = Model.BaseFactory.CreateSqLiteDatabase();
            long sizebefore = db.FileBase.Length;
            await db.VacuumAsync();
            long sizeafter = new FileInfo(db.FileBase.FullName).Length;
            Model.Info = string.Format("Database compacted (bytes): {0} -> {1}", sizebefore, sizeafter);
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

        private async void FocusRow(object obj)
        {
            await Model.SelectChannel();

            // focus
            var datagrid = obj as DataGrid;
            if (datagrid == null)
            {
                return;
            }
            datagrid.UpdateLayout();
            var selectedRow = (DataGridRow)datagrid.ItemContainerGenerator.ContainerFromIndex(datagrid.SelectedIndex);
            if (selectedRow == null)
            {
                return;
            }
            selectedRow.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void OpenDir(object obj)
        {
            switch (obj.ToString())
            {
                case "DirPath":
                    var dlg = new FolderBrowserDialog();
                    DialogResult res = dlg.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        Model.DirPath = dlg.SelectedPath;
                    }
                    break;

                case "MpcPath":
                    var dlgm = new OpenFileDialog { Filter = @"EXE files (*.exe)|*.exe" };
                    DialogResult resm = dlgm.ShowDialog();
                    if (resm == DialogResult.OK)
                    {
                        Model.MpcPath = dlgm.FileName;
                    }
                    break;

                case "YouPath":
                    var dlgy = new OpenFileDialog { Filter = @"EXE files (*.exe)|*.exe" };
                    DialogResult resy = dlgy.ShowDialog();
                    if (resy == DialogResult.OK)
                    {
                        Model.YouPath = dlgy.FileName;
                    }
                    break;
            }
        }

        #endregion
    }
}
