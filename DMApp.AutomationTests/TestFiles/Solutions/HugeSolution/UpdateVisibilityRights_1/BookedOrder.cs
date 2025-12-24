namespace UpdateVisibilityRights_1
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class BookedOrder
	{
		public BookedOrder(ReservationInstance reservation, Dictionary<int, ServiceConfiguration> serviceConfigurations)
		{
			Reservation = reservation;
			ServiceConfigurations = serviceConfigurations;
		}

		public ReservationInstance Reservation { get; private set; }

		public List<BookedService> Services { get; private set; } = new List<BookedService>();

		public Dictionary<int, ServiceConfiguration> ServiceConfigurations { get; private set; }

		public Job Job { get; set; }

		public HashSet<int> SecurityViewIds
		{
			get
			{
				if (Reservation == null) return new HashSet<int>();
				return new HashSet<int>(Reservation.SecurityViewIDs);
			}
		}

		public Guid JobId
		{
			get
			{
				if (Reservation == null) return Guid.Empty;

				object jobId;
				if (!Reservation.Properties.Dictionary.TryGetValue("EventId", out jobId)) return Guid.Empty;

				Guid result;
				if (!Guid.TryParse(Convert.ToString(jobId), out result)) return Guid.Empty;

				return result;
			}
		}
	}
}