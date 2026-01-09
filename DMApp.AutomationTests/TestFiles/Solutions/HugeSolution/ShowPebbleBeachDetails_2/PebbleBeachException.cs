using System;
using System.Runtime.Serialization;

namespace ShowPebbleBeachDetails_2
{
	[Serializable]
	public class PebbleBeachException : Exception
	{
		public PebbleBeachException() : base("Encountered an error with the Plasma Element")
		{
		}

		public PebbleBeachException(string message) : base(message)
		{
		}

		public PebbleBeachException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected PebbleBeachException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}