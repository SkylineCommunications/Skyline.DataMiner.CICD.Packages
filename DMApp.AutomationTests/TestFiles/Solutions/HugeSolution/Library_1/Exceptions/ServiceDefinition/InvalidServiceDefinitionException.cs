using System;
using System.Collections.Generic;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceDefinition
{
	public class InvalidServiceDefinitionException : MediaServicesException
	{
		public InvalidServiceDefinitionException()
		{
		}

		public InvalidServiceDefinitionException(string message) : base(message)
		{
		}

		public InvalidServiceDefinitionException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
