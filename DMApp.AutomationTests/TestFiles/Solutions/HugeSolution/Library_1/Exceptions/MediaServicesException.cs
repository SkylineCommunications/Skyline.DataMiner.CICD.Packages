namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;
	using System.Runtime.Serialization;

	[Serializable]
	public abstract class MediaServicesException : Exception
	{
		protected MediaServicesException()
		{
		}

		protected MediaServicesException(string message)
			: base(message)
		{
		}

		protected MediaServicesException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected MediaServicesException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}