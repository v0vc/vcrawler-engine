// This file contains my intellectual property. Release of this file requires prior approval from me.
// 
// 
// Copyright (c) 2015, v0v All Rights Reserved

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Extensions
{
    public static class StringExtensions
    {
        #region Constants

        private const string newLine = "\r\n";

        #endregion

        #region Static Methods

        public static string TimeAgo(DateTime dt)
        {
            TimeSpan span = DateTime.Now - dt;
            if (span.Days > 365)
            {
                int years = span.Days / 365;
                if (span.Days % 365 != 0)
                {
                    years += 1;
                }
                return string.Format("about {0} {1} ago", years, years == 1 ? "year" : "years");
            }
            if (span.Days > 30)
            {
                int months = span.Days / 30;
                if (span.Days % 31 != 0)
                {
                    months += 1;
                }
                return string.Format("about {0} {1} ago", months, months == 1 ? "month" : "months");
            }
            if (span.Days > 0)
            {
                return string.Format("about {0} {1} ago", span.Days, span.Days == 1 ? "day" : "days");
            }
            if (span.Hours > 0)
            {
                return string.Format("about {0} {1} ago", span.Hours, span.Hours == 1 ? "hour" : "hours");
            }
            if (span.Minutes > 0)
            {
                return string.Format("about {0} {1} ago", span.Minutes, span.Minutes == 1 ? "minute" : "minutes");
            }
            if (span.Seconds > 5)
            {
                return string.Format("about {0} seconds ago", span.Seconds);
            }
            if (span.Seconds <= 5)
            {
                return "just now";
            }
            return string.Empty;
        }

        public static string IntTostrTime(int duration)
        {
            TimeSpan t = TimeSpan.FromSeconds(duration);
            if (t.Days > 0)
            {
                return $"{t.Days:D2}:{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
            }
            return t.Hours > 0 ? $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}" : $"{t.Minutes:D2}:{t.Seconds:D2}";
        }

        public static string AviodTooLongFileName(this string path)
        {
            return path.Length > 240 ? path.Remove(240) : path;
        }

        public static bool IsValidUrl(this string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                return false;
            }
            Uri uri;
            return Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri) && null != uri;
        }

        public static string MakeValidFileName(this string name)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(String.Format("[{0}]", Regex.Escape(regexSearch)));
            string s = r.Replace(name, String.Empty);
            s = Regex.Replace(s, @"\s{2,}", " ");
            return s;
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            return String.IsNullOrEmpty(str) ? String.Empty : Regex.Replace(str, "[^a-zA-Z0-9_.]+", String.Empty, RegexOptions.Compiled);
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

                        while (pos < eol && Char.IsWhiteSpace(theString[pos]))
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
            while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
            {
                i--;
            }
            if (i < 0)
            {
                return max; // No whitespace found; break at maximum length
            }

            // Find start of whitespace
            while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
            {
                i--;
            }

            // Return length of text before whitespace
            return i + 1;
        }

        #endregion
    }
}
