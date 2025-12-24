using System;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ServiceConfigurations
{
	public class ServiceConfigurationNotFoundException : Exception
	{
		public ServiceConfigurationNotFoundException()
		{
		}

		public ServiceConfigurationNotFoundException(Guid orderId)
			: base($"Unable to find service configurations for order ID {orderId}")
		{
		}

		public ServiceConfigurationNotFoundException(string orderName)
			: base($"Unable to find service configurations for order {orderName}")
		{
		}

		public ServiceConfigurationNotFoundException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
