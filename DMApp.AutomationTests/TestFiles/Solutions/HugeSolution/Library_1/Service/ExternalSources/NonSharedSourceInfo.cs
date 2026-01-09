namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ExternalSources
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class NonSharedSourceInfo : ExternalSourceInfo
	{
		public NonSharedSourceInfo(Helpers helpers, ServiceReservationInstance reservationInstance) : base(helpers, reservationInstance)
		{
		}

		public override string DropDownOption => GetDropDownOption();

		private string GetDropDownOption()
		{
			string orderName;
			if (Reservation.Properties.ContainsKey(ServicePropertyNames.OrderNamePropertyName))
			{
				var orderNameProperty = Reservation.Properties.First(x => x.Key.Equals(ServicePropertyNames.OrderNamePropertyName));
				orderName = Convert.ToString(orderNameProperty.Value);
				if (String.IsNullOrWhiteSpace(orderName)) orderName = OrderName;
			}
			else
			{
				orderName = OrderName;
			}

			return $"{orderName} [{ShortDescription}]";
		}
	}
}
