namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;
	using System.Runtime.Serialization;

	[Serializable]
	public class EurovisionSynopsisException : Exception
	{
		public EurovisionSynopsisException()
		{
		}

		public EurovisionSynopsisException(string message)
			: base(message)
		{
		}

		public EurovisionSynopsisException(string synopsisId, string message)
			: this($"Synopsis {synopsisId}, {message}")
		{
		}

		public EurovisionSynopsisException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected EurovisionSynopsisException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
