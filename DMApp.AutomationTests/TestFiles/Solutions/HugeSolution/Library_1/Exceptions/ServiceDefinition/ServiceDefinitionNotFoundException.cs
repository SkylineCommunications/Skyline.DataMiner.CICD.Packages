using System;
using System.Collections.Generic;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceDefinition
{
	public class ServiceDefinitionNotFoundException : MediaServicesException
	{
		public ServiceDefinitionNotFoundException()
		{
		}

		public ServiceDefinitionNotFoundException(string message) : base(message)
		{
		}

		public ServiceDefinitionNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
