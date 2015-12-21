// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Interfaces.Enums;

namespace Extensions
{
    public static class CommonExtensions
    {
        #region Constants

        public const string YouRegex = @"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)";

        #endregion

        #region Static Methods

        public static string GetConsoleOutput(string path, string param, bool isClear)
        {
            var pProcess = new Process
            {
                StartInfo =
                {
                    FileName = path,
                    Arguments = param,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            try
            {
                pProcess.Start();
                string res = pProcess.StandardOutput.ReadToEnd();
                pProcess.Close();
                return isClear ? res.MakeValidFileName() : res;
            }
            catch
            {
                return "Please, check";
            }
        }

        public static string GetFileVersion(Assembly assembly)
        {
            // Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return "Crawler v" + fvi.FileVersion;
        }

        public static string GetSiteAdress(SiteType site)
        {
            switch (site)
            {
                case SiteType.YouTube:
                    return "youtube.com";
                case SiteType.Tapochek:
                    return "tapochek.net";
                case SiteType.RuTracker:
                    return "rutracker.org";
                default:
                    return string.Empty;
            }
        }

        public static SiteType GetSiteType(string site)
        {
            switch (site)
            {
                case "youtube.com":
                    return SiteType.YouTube;
                case "tapochek.net":
                    return SiteType.Tapochek;
                case "rutracker.org":
                    return SiteType.RuTracker;
                default:
                    return SiteType.NotSet;
            }
        }

        public static bool RenameFile(FileInfo oldFile, FileInfo newFile)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    File.Move(oldFile.FullName, newFile.FullName);
                    return true;
                }
                catch
                {
                    Thread.Sleep(1000);
                    i++;
                }
            }
            return false;
        }

        #endregion
    }
}
