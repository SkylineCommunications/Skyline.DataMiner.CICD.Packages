using System;
using System.Collections.Generic;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceDefinition
{
	public class NodeIdMismatchException : MediaServicesException
	{
		public static readonly string DefaultMessage = "Node ID mismatch";

		public NodeIdMismatchException() : base(DefaultMessage)
		{
		}

		public NodeIdMismatchException(string message) : base(message)
		{
		}

		public NodeIdMismatchException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
