using System;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.CodeAnalysis.CSharp;

namespace IntelliTect.Coalesce.Utilities
{
    public static class StringExtensions
    {
        // Convert the string to Pascal case.
#if NETCOREAPP
        [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("theString")]
#endif
        public static string? ToPascalCase(this string? theString)
        {
            // If there are 0 or 1 characters, just return the string.
            if (theString == null) return null;
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

        /// <summary>
        ///     Convert the string to camel case.
        /// </summary>
        /// <remarks>
        ///     This is largely taken from JSON.NET's camelCase converter, 
        ///     with some modifications to remove cases we don't need to handle 
        ///     (e.g. whitespace in the middle of identifiers)
        /// </remarks>
        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    break;
                }

                chars[i] = char.ToLowerInvariant(chars[i]);
            }

            return new string(chars);
        }

        // Capitalize the first character and add a space before
        // each capitalized letter (except the first character).
#if NETCOREAPP
        [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("theString")]
#endif
        public static string? ToProperCase(this string? theString)
        {
            // If there are 0 or 1 characters, just return the string.
            if (theString == null) return null;
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

#if NETCOREAPP
        [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("str")]
#endif
        public static string? EscapeStringLiteralForLinqDynamic(this string? str) => str?
            .Replace(@"\", @"\\")
            .Replace("\"", "\\\"");

#if NETCOREAPP
        [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("str")]
#endif
        public static string? EscapeStringLiteralForCSharp(this string? str) => str?
            .Replace(@"\", @"\\")
            .Replace("\"", "\\\"");

#if NETCOREAPP
        [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("str")]
#endif
        public static string? QuotedStringLiteralForCSharp(this string? str) => 
            str is null ? null : ('"' + str?
            .Replace(@"\", @"\\")
            .Replace("\"", "\\\"")
            + '"');

#if NETCOREAPP
        [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("str")]
#endif
        public static string? EscapeStringLiteralForTypeScript(this string? str) => str?
            .Replace(@"\", @"\\")
            .Replace("\"", "\\\"");

#if NETCOREAPP
        [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("str")]
#endif
        public static string? EscapeForHtml(this string? str) =>
            str == null ? null : HttpUtility.HtmlEncode(str);

        /// <summary>
        /// Convert a string to a valid C# identifier that conforms to language specification 2.4.2
        /// </summary>
        /// <param name="string">The string to convert</param>
        /// <param name="prefix">(Optional) string to prefix the identifier with.</param>
        /// <returns>A valid C# identifier</returns>
        public static string GetValidCSharpIdentifier(this string @string, string? prefix = null)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                @string = prefix + @string;
            }
            @string = @string.Trim();

            @string = Regex.Replace(@string, @"(?!\\u[0-9A-Fa-f]{4})[^\w]", match =>
            {
                if (match.Index == 0 && match.Value == "@")
                    return "@";
                return @"_";
            });

            if (!Regex.IsMatch(@string, "^[_@a-zA-Z]"))
            {
                @string = $"_{@string}";
            }
            else if (SyntaxFacts.GetKeywordKind(@string) != SyntaxKind.None)
            {
                @string = $"@{@string}";
            }

            return @string;
        }
    }
}