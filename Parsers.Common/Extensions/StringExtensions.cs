namespace Skyline.DataMiner.CICD.Parsers.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Defines extension methods on the <see cref="String"/> and <see cref="StringBuilder"/> class.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Reads the lines of the specified string.
        /// </summary>
        /// <param name="s">The string to read the lines from.</param>
        /// <returns>The lines of the string.</returns>
        public static IEnumerable<string> ReadLines(this string s)
        {
            if (s == null)
            {
                return Enumerable.Empty<string>();
            }
            else
            {
                return s.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified StringBuilder content ends with the specified string.
        /// </summary>
        /// <param name="b">The StringBuilder instance.</param>
        /// <param name="s">The string value.</param>
        /// <returns><c>true</c> if the StringBuilder instance ends with the specified string; otherwise, <c>false</c>.</returns>
        public static bool EndsWith(this StringBuilder b, string s)
        {
            if (b.Length < s.Length)
            {
                return false;
            }

            int offset = b.Length - s.Length;

            for (int i = 0; i < s.Length; i++)
            {
                if (b[offset + i] != s[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a value indicating whether the specified StringBuilder content starts with the specified string.
        /// </summary>
        /// <param name="b">The StringBuilder instance.</param>
        /// <param name="s">The string value.</param>
        /// <returns><c>true</c> if the StringBuilder instance starts with the specified string; otherwise, <c>false</c>.</returns>
        public static bool StartsWith(this StringBuilder b, string s)
        {
            if (b.Length < s.Length)
            {
                return false;
            }

            for (int i = 0; i < s.Length; i++)
            {
                if (b[i] != s[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Retrieves a substring from the specified <see cref="StringBuilder"/> instance.
        /// </summary>
        /// <param name="b">The <see cref="StringBuilder"/> instance.</param>
        /// <param name="startIndex">The zero-based starting character position of a substring in the <see cref="StringBuilder"/> instance.</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns>A string that is equivalent to the substring of length <paramref name="length"/> that begins at <paramref name="startIndex"/> in this instance, or Empty if startIndex is equal to the length of this instance and length is zero.</returns>
        public static string Substring(this StringBuilder b, int startIndex, int length)
        {
            return b.ToString(startIndex, length);
        }

        /// <summary>
        /// Retrieves the position of the last non whitespace character of the specified string.
        /// </summary>
        /// <param name="s">The string for which to obtain the last non whitespace character.</param>
        /// <returns>The position of the last non whitespace character of the specified string.</returns>
        public static int GetLastNonWhiteSpacePosition(this string s)
        {
            for (int i = s.Length - 1; i >= 0; i--)
            {
                if (!Char.IsWhiteSpace(s[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Calls .Substring() but clips to bounds. Note that a negative offset will reduce the given length.
        /// </summary>
        /// <param name="s">The input <see cref="String"/>.</param>
        /// <param name="offset">The zero-based starting character position of a substring in <paramref name="s"/>.</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns>A string that is equivalent to the substring of length <paramref name="length"/> that begins at <paramref name="offset"/> in this instance, or Empty if <paramref name="offset"/> is equal to the length of this instance and length is zero.</returns>
        public static string SafeSubstring(this string s, int offset, int length)
        {
            return new StringBuilder(s).SafeSubstring(offset, length);
        }

        /// <summary>
        /// Calls .Substring() but clips to bounds. Note that a negative offset will reduce the given length.
        /// </summary>
        /// <param name="sb">The input <see cref="StringBuilder"/>.</param>
        /// <param name="offset">The zero-based starting character position of a substring in <paramref name="sb"/>.</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns>A string that is equivalent to the substring of length <paramref name="length"/> that begins at <paramref name="offset"/> in this instance, or Empty if <paramref name="offset"/> is equal to the length of this instance and length is zero.</returns>
        public static string SafeSubstring(this StringBuilder sb, int offset, int length)
        {
            if (length <= 0 || offset >= sb.Length)
            {
                return String.Empty;
            }

            if (offset < 0)
            {
                length += offset;
                offset = 0;
            }

            if (length > sb.Length - offset)
            {
                length = sb.Length - offset;
            }

            return sb.Substring(offset, length);
        }
    }
}
