namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class ElementNotActiveException : MediaServicesException
	{
		public ElementNotActiveException()
		{
		}

		public ElementNotActiveException(string name)
			: base($"Element {name} is inactive")
		{
		}

		public ElementNotActiveException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}