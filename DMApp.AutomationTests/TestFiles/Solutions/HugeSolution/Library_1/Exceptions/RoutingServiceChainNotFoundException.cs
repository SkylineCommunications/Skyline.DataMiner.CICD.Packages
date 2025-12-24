using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions
{
	public class RoutingServiceChainNotFoundException : Exception
	{
		public RoutingServiceChainNotFoundException(string usingRoutingServiceName) : base($"Unable to find Routing Service Chain that uses routing service {usingRoutingServiceName}")
		{

		}
	}
}
