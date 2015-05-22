using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Extensions
{
    public static class StringExtensions
    {
        private const string Newline = "\r\n";

        private const int Width = 150;

        public static string WordWrap(this string theString)
        {

            int pos, next;
            var sb = new StringBuilder();

            // Lucidity check
            //if (Width < 1)
            //    return theString;

            // Parse each line of text
            for (pos = 0; pos < theString.Length; pos = next)
            {
                // Find end of line
                int eol = theString.IndexOf(Newline, pos, StringComparison.Ordinal);

                if (eol == -1)
                    next = eol = theString.Length;
                else
                    next = eol + Newline.Length;

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        int len = eol - pos;

                        if (len > Width)
                            len = BreakLine(theString, pos, Width);

                        sb.Append(theString, pos, len);
                        sb.Append(Newline);

                        // Trim whitespace following break
                        pos += len;

                        while (pos < eol && Char.IsWhiteSpace(theString[pos]))
                            pos++;

                    } while (eol > pos);
                }
                else sb.Append(Newline); // Empty line
            }

            return sb.ToString();
        }

        public static int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            int i = max - 1;
            while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
                i--;
            if (i < 0)
                return max; // No whitespace found; break at maximum length
            // Find start of whitespace
            while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
                i--;
            // Return length of text before whitespace
            return i + 1;
        }
    }
}
