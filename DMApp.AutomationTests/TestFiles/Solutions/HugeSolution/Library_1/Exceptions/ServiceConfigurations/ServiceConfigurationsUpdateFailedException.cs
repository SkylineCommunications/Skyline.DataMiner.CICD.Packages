using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	public class ServiceConfigurationsUpdateFailedException : Exception
	{
		public ServiceConfigurationsUpdateFailedException()
		{
		}

		public ServiceConfigurationsUpdateFailedException(Guid orderId)
			: base($"Unable to update service configurations for order ID {orderId}")
		{
		}

		public ServiceConfigurationsUpdateFailedException(string orderName)
			: base($"Unable to update service configurations for order {orderName}")
		{
		}

		public ServiceConfigurationsUpdateFailedException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
