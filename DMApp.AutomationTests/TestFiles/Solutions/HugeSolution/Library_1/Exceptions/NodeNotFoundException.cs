namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	using System;

	public class NodeNotFoundException : MediaServicesException
	{
		public NodeNotFoundException()
		{
		}

		public NodeNotFoundException(string message)
			: base(message)
		{
		}

		public NodeNotFoundException(Guid ID)
			: base($"Unable to find Function with ID {ID}")
		{
		}

		public NodeNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}