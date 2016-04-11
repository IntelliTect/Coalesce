using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intellitect.NLog.Extensions
{
    /// <summary>
    /// URL Encoding helper.
    /// </summary>
    internal class UrlHelper
    {
        private static string safeUrlPunctuation = ".()*-_!'";
        private static string hexChars = "0123456789abcdef";

        internal static string UrlEncode(string str, bool spaceAsPlus)
        {
            StringBuilder result = new StringBuilder(str.Length + 20);
            for (int i = 0; i < str.Length; ++i)
            {
                char ch = str[i];

                if (ch == ' ' && spaceAsPlus)
                {
                    result.Append('+');
                }
                else if (IsSafeUrlCharacter(ch))
                {
                    result.Append(ch);
                }
                else if (ch < 256)
                {
                    result.Append('%');
                    result.Append(hexChars[(ch >> 4) & 0xF]);
                    result.Append(hexChars[(ch >> 0) & 0xF]);
                }
                else
                {
                    result.Append('%');
                    result.Append('u');
                    result.Append(hexChars[(ch >> 12) & 0xF]);
                    result.Append(hexChars[(ch >> 8) & 0xF]);
                    result.Append(hexChars[(ch >> 4) & 0xF]);
                    result.Append(hexChars[(ch >> 0) & 0xF]);
                }
            }

            return result.ToString();
        }

        private static bool IsSafeUrlCharacter(char ch)
        {
            if (ch >= 'a' && ch <= 'z')
            {
                return true;
            }

            if (ch >= 'A' && ch <= 'Z')
            {
                return true;
            }

            if (ch >= '0' && ch <= '9')
            {
                return true;
            }

            return safeUrlPunctuation.IndexOf(ch) >= 0;
        }
    }
}
