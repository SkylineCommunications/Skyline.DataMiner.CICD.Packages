namespace UpdateVisibilityRights_1
{
	using System.Collections.Generic;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class BookedService
	{
		public BookedService(ReservationInstance reservation)
		{
			Reservation = reservation;
		}

		public ReservationInstance Reservation { get; private set; }

		public HashSet<int> SecurityViewIds()
		{
			if (Reservation == null) return new HashSet<int>();
			return new HashSet<int>(Reservation.SecurityViewIDs);
		}
	}
}