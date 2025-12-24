namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ExternalSources
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class SharedSourceInfo : ExternalSourceInfo
	{
		public SharedSourceInfo(Helpers helpers, ServiceReservationInstance reservationInstance) : base(helpers, reservationInstance)
		{
		}

		public override string DropDownOption => $"{OrderName} [{ShortDescription}]";
	}
}
