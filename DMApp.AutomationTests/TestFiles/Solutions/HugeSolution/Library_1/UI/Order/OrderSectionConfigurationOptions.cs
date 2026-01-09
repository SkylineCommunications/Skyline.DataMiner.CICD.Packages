namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;

	public class OrderSectionConfigurationOptions
	{
		public bool IsReadOnly { get; set; }

		public Scripts Script { get; set; }

		public EditOrderFlows Flow { get; set; }

		public Service ServiceBeingEdited { get; set; }
	}
}
