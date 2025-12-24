namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System.Linq;
	using System.Text;
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public static class StringExtensions
	{
		// Illegal Characters
		private const int Backslash = '/';

		private const int Forwardslash = '\\';

		private const int Colon = ':';

		private const int Asterisk = '*';

		private const int QuestionMark = '?';

		private const int DoubleQuotationMark = '"';

		private const int LessThan = '<';

		private const int GreaterThan = '>';

		private const int Pipe = '|';

		private const int Degree = 248; // '°' gives issues in DIS

		private const int Semicolon = ';';

		private static readonly int[] IllegalCharacters = new[] { Backslash, Forwardslash, Colon, Asterisk, QuestionMark, DoubleQuotationMark, LessThan, GreaterThan, Pipe, Degree, Semicolon };

		public static bool ContainsIgnoreCase(this string thisString, string s)
		{
			return thisString.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static T GetEnumValue<T>(this string value)
		{
			var type = typeof(T);
			if (!type.IsEnum) throw new ArgumentException();

			FieldInfo[] fields = type.GetFields();
			var field = fields.SelectMany(f => f.GetCustomAttributes(typeof(DescriptionAttribute), false), (f, a) => new { Field = f, Att = a }).SingleOrDefault(a => ((DescriptionAttribute)a.Att).Description == value);

			return field == null ? default(T) : (T)field.Field.GetRawConstantValue();
		}

		public static bool IsValidEnumDescription<T>(this string value)
		{
			var type = typeof(T);
			if (!type.IsEnum) throw new ArgumentException();

			var field = type.GetFields().SelectMany(f => f.GetCustomAttributes(typeof(DescriptionAttribute), false), (f, a) => new { Field = f, Att = a }).SingleOrDefault(a => ((DescriptionAttribute)a.Att).Description == value);

			return field != null;
		}

		public static string SplitCamelCase(this string input)
		{
			return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
		}

		/// <summary>
		/// Removes any illegal character from a string.
		/// </summary>
		/// <param name="value">String to clean.</param>
		/// <param name="allowSiteContent">Will remain all special characters which are present inside a site url.</param>
		/// <returns>String without any illegal characters.</returns>
		public static string Clean(this string value, bool allowSiteContent = false)
		{
			if (String.IsNullOrWhiteSpace(value)) return String.Empty;

			bool previousCharacterWasSpace = false;
			StringBuilder sb = new StringBuilder();
			foreach (char c in value)
			{
				if (c < 32 || (!allowSiteContent && IllegalCharacters.Contains(c))) continue;

				bool currentCharacterIsSpace = c == 32;
				if (previousCharacterWasSpace && currentCharacterIsSpace)
				{
					// Only keep 1 space in case there are multiple next to eachother
				}
				else
				{
					sb.Append(c);
				}

				previousCharacterWasSpace = currentCharacterIsSpace;
			}

			return sb.ToString().Trim(' ');
		}

        /// <summary>
		/// Checks if a string contains illegal characters
		/// </summary>
		/// <param name="value">String to check.</param>
		/// <returns>True when string contains any illegal character otherwise False.</returns>
		public static bool ContainsIllegalCharacters(this string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return false;

            foreach (char c in value)
            {
                if (IllegalCharacters.Contains(c))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
		/// Filter out trailing escaped characters from string
		/// </summary>
		/// <param name="value">String to check.</param>
		/// <returns>Returns a string without trailing escaped characters if any before, else returns the string unchanged.</returns>
		public static string FilterOutTrailingEscapedCharacters(this string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return String.Empty;

            value = Regex.Unescape(value);

            StringBuilder sb = new StringBuilder();
            for (int i = value.Length - 1; i >= 0; i--)
            {
                if (value[i] >= 32)
                {
                    // First occurrence of a valid char at the end.
                    break;
                }

               sb.Append(value[i]);
            }

            string result = value.Remove(value.Length - sb.Length, sb.Length);

            return result.Trim(' ');
        }

        public static string CleanXml(this string value)
		{
			bool isSpace = false;
			StringBuilder sb = new StringBuilder();
			foreach (char c in value)
			{
				if (c < 32) continue;
				if (isSpace && c == 32)
				{
					// Skip space
				}
				else
				{
					sb.Append(c);
				}

				isSpace = c == 32;
			}

			return sb.ToString();
		}
	}
}