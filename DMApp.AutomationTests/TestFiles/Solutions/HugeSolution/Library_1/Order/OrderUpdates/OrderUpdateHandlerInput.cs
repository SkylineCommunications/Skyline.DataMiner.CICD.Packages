using System;
using System.Collections.Generic;
using System.Text;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates
{
	public class OrderUpdateHandlerInput
	{
		public OrderUpdateHandler.OptionFlags Options { get; set; }

		public bool IsHighPriority { get; set; }

		public bool ProcessChronologically { get; set; }

		public Order ExistingOrder { get; set; }
	}
}
