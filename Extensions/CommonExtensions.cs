// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media.Imaging;
using Interfaces.Enums;

namespace Extensions
{
    public static class CommonExtensions
    {
        #region Constants

        public const string YouRegex = @"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)";
        private const string newLine = "\r\n";

        #endregion

        #region Static Methods

        public static string AviodTooLongFileName(string path)
        {
            return path.Length > 240 ? path.Remove(240) : path;
        }

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

        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            Uri uri;
            return Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri) && null != uri;
        }

        public static bool IsUrlExist(string url)
        {
            try
            {
                var request = WebRequest.Create(url) as HttpWebRequest;
                if (request != null)
                {
                    request.Method = "HEAD";
                    var response = request.GetResponse() as HttpWebResponse;
                    if (response != null)
                    {
                        response.Close();
                        return response.StatusCode == HttpStatusCode.OK;
                    }
                }
            }
            catch 
            {
                return false;
            }
            return true;
        }

        public static string MakeValidFileName(this string name)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            string s = r.Replace(name, string.Empty);
            s = Regex.Replace(s, @"\s{2,}", " ");
            return s;
        }

        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", string.Empty, RegexOptions.Compiled);
        }

        public static bool RenameFile(FileInfo oldFile, FileInfo newFile)
        {
            for (var i = 0; i < 10; i++)
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

        public static IEnumerable<List<string>> SplitList(List<string> locations, int nSize = 50)
        {
            var list = new List<List<string>>();

            for (var i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }

        public static BitmapImage ToImage(byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        public static string WordWrap(this string theString, int width)
        {
            int pos, next;
            var sb = new StringBuilder();

            // Lucidity check
            // if (Width < 1)
            // return theString;

            // Parse each line of text
            for (pos = 0; pos < theString.Length; pos = next)
            {
                // Find end of line
                int eol = theString.IndexOf(newLine, pos, StringComparison.Ordinal);

                if (eol == -1)
                {
                    next = eol = theString.Length;
                }
                else
                {
                    next = eol + newLine.Length;
                }

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        int len = eol - pos;

                        if (len > width)
                        {
                            len = BreakLine(theString, pos, width);
                        }

                        sb.Append(theString, pos, len);
                        sb.Append(newLine);

                        // Trim whitespace following break
                        pos += len;

                        while (pos < eol && char.IsWhiteSpace(theString[pos]))
                        {
                            pos++;
                        }
                    }
                    while (!(eol <= pos));
                }
                else
                {
                    sb.Append(newLine); // Empty line
                }
            }

            return sb.ToString();
        }

        private static int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            int i = max - 1;
            while (i >= 0 && !char.IsWhiteSpace(text[pos + i]))
            {
                i--;
            }
            if (i < 0)
            {
                return max; // No whitespace found; break at maximum length
            }

            // Find start of whitespace
            while (i >= 0 && char.IsWhiteSpace(text[pos + i]))
            {
                i--;
            }

            // Return length of text before whitespace
            return i + 1;
        }

        #endregion
    }
}
