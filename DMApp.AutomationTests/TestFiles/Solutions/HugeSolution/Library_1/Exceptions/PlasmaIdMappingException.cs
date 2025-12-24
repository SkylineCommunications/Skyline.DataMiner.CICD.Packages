namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;
	using System.Runtime.Serialization;

	[Serializable]
	public class PlasmaIdMappingException : Exception
	{
		public PlasmaIdMappingException()
		{
		}

		public PlasmaIdMappingException(string message)
			: base(message)
		{
		}

		public PlasmaIdMappingException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected PlasmaIdMappingException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
