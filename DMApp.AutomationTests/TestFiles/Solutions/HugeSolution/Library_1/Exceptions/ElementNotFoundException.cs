namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM;

	public class ElementNotFoundException : MediaServicesException
	{
		public ElementNotFoundException()
		{
		}

		public ElementNotFoundException(string protocol)
			: base($"Unable to find Element with protocol {protocol}")
		{
		}

		public ElementNotFoundException(string protocol, InterplayPamElements elementName)
			: base($"Unable to find {protocol} {elementName.ToString()} Element")
		{
		}

		public ElementNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}