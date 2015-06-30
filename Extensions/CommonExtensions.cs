using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media.Imaging;

namespace Extensions
{
    public static class CommonExtensions
    {

        //private const string Newline = "\r\n";

        public static string MakeValidFileName(this string name)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            var s = r.Replace(name, String.Empty);
            s = Regex.Replace(s, @"\s{2,}", " ");
            return s;
        }

        public static string GetVersion(string path, string param)
        {
            var pProcess = new System.Diagnostics.Process
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
                var res = pProcess.StandardOutput.ReadToEnd();
                pProcess.Close();
                return res.MakeValidFileName();
            }
            catch
            {
                return "Please, check";
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

        //public static string WordWrap(this string theString, int width)
        //{

        //    int pos, next;
        //    var sb = new StringBuilder();

        //    // Lucidity check
        //    //if (Width < 1)
        //    //    return theString;

        //    // Parse each line of text
        //    for (pos = 0; pos < theString.Length; pos = next)
        //    {
        //        // Find end of line
        //        int eol = theString.IndexOf(Newline, pos, StringComparison.Ordinal);

        //        if (eol == -1)
        //            next = eol = theString.Length;
        //        else
        //            next = eol + Newline.Length;

        //        // Copy this line of text, breaking into smaller lines as needed
        //        if (eol > pos)
        //        {
        //            do
        //            {
        //                int len = eol - pos;

        //                if (len > width)
        //                    len = BreakLine(theString, pos, width);

        //                sb.Append(theString, pos, len);
        //                sb.Append(Newline);

        //                // Trim whitespace following break
        //                pos += len;

        //                while (pos < eol && Char.IsWhiteSpace(theString[pos]))
        //                    pos++;

        //            } while (!(eol <= pos));
        //        }
        //        else sb.Append(Newline); // Empty line
        //    }

        //    return sb.ToString();
        //}

        //private static int BreakLine(string text, int pos, int max)
        //{
        //    // Find last whitespace in line
        //    int i = max - 1;
        //    while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
        //        i--;
        //    if (i < 0)
        //        return max; // No whitespace found; break at maximum length
        //    // Find start of whitespace
        //    while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
        //        i--;
        //    // Return length of text before whitespace
        //    return i + 1;
        //}

        public static string GetFileVersion(Assembly assembly)
        {
            //Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return "Crawler v" + fvi.FileVersion;
        }

        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri) || null == uri)
            {
                return false;
            }
            return true;
        }

        public static List<List<string>> SplitList(List<string> locations, int nSize = 50)
        {
            var list = new List<List<string>>();

            for (int i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }
    }
}
