using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IntelliTect.Coalesce.Utilities
{
    public static class StringExtensions
    {
        // Convert the string to Pascal case.
        public static string ToPascalCase(this string theString)
        {
            // If there are 0 or 1 characters, just return the string.
            if (theString == null) return theString;
            if (theString.Length < 2) return theString.ToUpper();

            // Split the string into words.
            string[] words = theString.Split(
                new char[] { },
                StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = "";
            foreach (string word in words)
            {
                result +=
                    word.Substring(0, 1).ToUpper() +
                    word.Substring(1);
            }

            return result;
        }

        // Convert the string to camel case.
        public static string ToCamelCase(this string theString)
        {
            if (theString == null) return theString;
            if (theString.Length <= 2) return theString.ToLower();
            else return theString.Substring(0, 1).ToLower() + theString.Substring(1);

            //// If there are 0 or 1 characters, just return the string.
            //if (theString == null || theString.Length < 2)
            //    return theString;

            //Regex r = new Regex("([A-Z]+[a-z]+)");
            //string spaced = r.Replace(theString, m => m + " ");

            //// Split the string into words.
            //string[] words = spaced.Split(
            //    new char[] {' '},
            //    StringSplitOptions.RemoveEmptyEntries);

            //// Combine the words.
            //string result = words[0].ToLower();
            //for (int i = 1; i < words.Length; i++)
            //{
            //    result +=
            //        words[i].Substring(0, 1).ToUpper() +
            //        words[i].Substring(1);
            //}

            //return result;
        }

        // Capitalize the first character and add a space before
        // each capitalized letter (except the first character).
        public static string ToProperCase(this string theString)
        {
            // If there are 0 or 1 characters, just return the string.
            if (theString == null) return theString;
            if (theString.Length < 2) return theString.ToUpper();

            // Start with the first character.
            string result = theString.Substring(0, 1).ToUpper();

            // Add the remaining characters.
            for (int i = 1; i < theString.Length; i++)
            {
                if (char.IsUpper(theString[i])) result += " ";
                result += theString[i];
            }

            return result;
        }

        /// <summary>
        /// Adds the items in the colleciton to list if they aren't already there.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="items"></param>
        public static void AddUnique(this List<string> list, IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                list.AddUnique(item);
            }
        }

        /// <summary>
        /// Adds the items in the colleciton to list if they aren't already there.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="items"></param>
        public static void AddUnique(this List<string> list, string item)
        {
            if (!list.Contains(item)) list.Add(item);
        }

        public static string EscapeStringLiteralForLinqDynamic(this string str) => str?
            .Replace(@"\", @"\\")
            .Replace("\"", "\"\"");
    }
}