namespace ShowPebbleBeachDetails_2.PebbleBeach
{
	using System;
	using System.Runtime.Serialization;

	[Serializable]
	public class EnumParseException : Exception
	{
		public EnumParseException() : base("An error occurred while parsing the description")
		{
		}

		public EnumParseException(string message) : base(message)
		{
		}

		public EnumParseException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected EnumParseException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}