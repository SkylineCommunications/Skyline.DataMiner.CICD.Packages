using System;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.AvidInterplayPAM
{
	public class ElementDidNotLogRequestException : MediaServicesException
	{
		public ElementDidNotLogRequestException(string receivingElementName)
			: base($"Element {receivingElementName} did not log the request in the table.")
		{
		}

		public ElementDidNotLogRequestException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}