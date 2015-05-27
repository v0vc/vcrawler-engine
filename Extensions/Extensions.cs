using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Extensions
{
    public static class Extensions
    {
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
                    break;
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
    }
}
