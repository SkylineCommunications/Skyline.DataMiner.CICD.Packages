namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;
	using System.Runtime.Serialization;

	[Serializable]
	public class CustomPropertyUpdateFailedException : MediaServicesException
	{
		public CustomPropertyUpdateFailedException()
		{
		}

		public CustomPropertyUpdateFailedException(string name)
			: base($"Unable to update custom properties for {name}")
		{
		}

		public CustomPropertyUpdateFailedException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected CustomPropertyUpdateFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}