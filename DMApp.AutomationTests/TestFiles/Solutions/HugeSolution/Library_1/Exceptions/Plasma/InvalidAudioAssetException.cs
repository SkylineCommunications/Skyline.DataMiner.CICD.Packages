using System;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.Plasma
{
	public sealed class InvalidAudioAssetException : Exception
	{
		public InvalidAudioAssetException()
		{
		}

		public InvalidAudioAssetException(string message)
			: base(message)
		{
		}

		public InvalidAudioAssetException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
