namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;
	using System.Globalization;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	public static class DateTimeExtensions
	{
		private const string finnishDateFormat = "dd.MM.yyyy";
		private static readonly string format = "yyyy-MM-dd";

		public static double ConvertToCustomDatetimePropertyForReservation(this DateTime dateTime)
		{
			// milliseconds since 1970 UTC
			return Math.Round((dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
		}

		public static string ToFullDetailString(this DateTime dt)
		{
			return $"{dt.ToString("o", CultureInfo.InvariantCulture)} ({dt.Kind})";
		}

		public static string ToFinnishDateString(this DateTime dt)
		{
			return dt.ToString(finnishDateFormat);
		}

		public static string ToString(DateTime dt)
		{
			return dt.ToString(format);
		}

		public static DateTime? Parse(string dt)
		{
			DateTime datetime;
			if (DateTime.TryParseExact(dt, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out datetime))
				return datetime;
			else
				return null;
		}

        /// <summary>
		/// Used to convert a DateTime from a job to the correct Local Time.
		/// </summary>
		/// <param name="dateTime">DateTime as retrieved from a job instance.</param>
		/// <returns>Local Time from job.</returns>
		public static DateTime FromJob(this DateTime dateTime)
        {
            return new DateTime(dateTime.Ticks, DateTimeKind.Utc).ToLocalTime();
        }

        /// <summary>
        /// Used to convert a DateTime from a reservationInstance to the correct Local Time.
        /// </summary>
        /// <param name="dateTime">DateTime as retrieved from a reservation instance.</param>
        /// <returns>Local Time from reservation instance.</returns>
        public static DateTime FromReservation(this DateTime dateTime)
		{
			return new DateTime(dateTime.Ticks, DateTimeKind.Utc).ToLocalTime();
		}

		/// <summary>
		/// Used to convert a DateTime from a service configuration to the correct Local Time.
		/// </summary>
		/// <param name="dateTime">DateTime as retrieved from a service configuration.</param>
		/// <returns>Local Time from service configuration.</returns>
		public static DateTime FromServiceConfiguration(this DateTime dateTime)
		{
			return new DateTime(dateTime.Ticks, DateTimeKind.Utc).ToLocalTime();
		}

		public static DateTime Truncate(this DateTime dt, TimeSpan timeSpan)
		{
			if (timeSpan == TimeSpan.Zero) return dt;

			return dt.AddTicks(-(dt.Ticks % timeSpan.Ticks));
		}

        /// <summary>
        /// Creates a new date time instance without seconds.
        /// </summary>
        /// <param name="dt">date time including seconds.</param>
        /// <returns>Returns a new date time without seconds based on the given input.</returns>
		public static DateTime RoundToMinutes(this DateTime dt)
		{
			return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, dt.Kind);
		}

		/// <summary>
		/// Checks if the year, month, day, hour and minutes match of both date times.
		/// Introduced as the DateTime.Equals method compares ticks.
		/// </summary>
		/// <param name="dt">This date time.</param>
		/// <param name="other">Date time to compare with.</param>
		/// <returns>True, if the year, month, day, hour and minutes match of both date times.</returns>
		public static bool Matches(this DateTime dt, DateTime other)
		{
			DateTime dtUtc = dt.ToUniversalTime();
			DateTime otherUtc = other.ToUniversalTime();

			bool yearMatches = dtUtc.Year == otherUtc.Year;
			bool monthMatches = dtUtc.Month == otherUtc.Month;
			bool dayMatches = dtUtc.Day == otherUtc.Day;
			bool hourMatches = dtUtc.Hour == otherUtc.Hour;
			bool minuteMatches = dtUtc.Minute == otherUtc.Minute;

			return yearMatches && monthMatches && dayMatches && hourMatches && minuteMatches;
		}

		public class DateFormatConverter : IsoDateTimeConverter
		{
			public DateFormatConverter(string format)
			{
				DateTimeFormat = format;
			}
		}

		public class MicrosecondEpochConverter : DateTimeConverterBase
		{
			private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				writer.WriteRawValue(((DateTime)value - _epoch).TotalMilliseconds.ToString());
			}

			public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
			{
				if (reader.Value == null) { return null; }
				return _epoch.AddMilliseconds((long)reader.Value);
			}
		}
	}
}
