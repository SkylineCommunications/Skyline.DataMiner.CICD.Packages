namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class AudioChannelPairNotFoundException : MediaServicesException
	{
		public AudioChannelPairNotFoundException()
		{
		}

		public AudioChannelPairNotFoundException(int channel)
			: base($"Unable to find Audio Channel Pair {channel}")
		{
		}

		public AudioChannelPairNotFoundException(int channel, string serviceName)
			: base($"Unable to find Audio Channel Pair {channel} in service {serviceName}")
		{
		}

		public AudioChannelPairNotFoundException(string message, Exception inner)
			: base($"No Reservation Instance found with ID {message}", inner)
		{
		}
	}
}