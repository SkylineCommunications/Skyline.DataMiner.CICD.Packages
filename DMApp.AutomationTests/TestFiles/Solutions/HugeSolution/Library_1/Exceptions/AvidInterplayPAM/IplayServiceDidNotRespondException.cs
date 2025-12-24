namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.AvidInterplayPAM
{
	using System;

	public class IplayServiceDidNotRespondException : MediaServicesException
	{
		public IplayServiceDidNotRespondException(string iplayServiceName)
	: base($"{iplayServiceName} service did not respond in time.")
		{
		}

		public IplayServiceDidNotRespondException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
