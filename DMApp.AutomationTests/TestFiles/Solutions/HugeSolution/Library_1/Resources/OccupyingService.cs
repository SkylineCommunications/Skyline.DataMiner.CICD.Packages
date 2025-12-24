using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Net.ResourceManager.Objects;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources
{
	public class OccupyingService
	{
		public OccupyingService(ReservationInstance service, List<ReservationInstance> orders)
		{
			Service = service;
			Orders = orders;
		}

		public ReservationInstance Service { get; }

		public List<ReservationInstance> Orders { get; }

		public override string ToString()
		{
			return $"Service {Service.Name} from {Service.Start} ({Service.Start.Kind}) until {Service.End} ({Service.End.Kind}) used in Order{(Orders.Count > 1 ? "s" : String.Empty)} {string.Join(", ", Orders.Select(o => o.Name))}";
		}
	}

}
