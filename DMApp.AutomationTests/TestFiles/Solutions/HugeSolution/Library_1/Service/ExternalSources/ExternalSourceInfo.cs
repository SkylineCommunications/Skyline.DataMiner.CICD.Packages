namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ExternalSources
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.AccessControl;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reservations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public abstract class ExternalSourceInfo
	{
		protected readonly Helpers helpers;

		private DisplayedService service;
		private List<ReservationInstance> orderReservations;

		protected ExternalSourceInfo(Helpers helpers, ServiceReservationInstance reservationInstance)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			Reservation = reservationInstance ?? throw new ArgumentNullException(nameof(reservationInstance));
		}

		public ServiceReservationInstance Reservation { get; }

		public DateTime SharedSourceStartWithPreRoll => Reservation.Start.ToLocalTime().Add(Reservation.GetPreRoll());

		public DateTime SharedSourceEndWithPostRoll => Reservation.End.ToLocalTime().Subtract(Reservation.GetPostRoll());

		public DisplayedService Service
		{
			get
			{
				return service ?? (service = YLE.Service.Service.FromReservationInstance(helpers, Reservation));
			}
		}

		public abstract string DropDownOption { get; }

		protected List<ReservationInstance> OrderReservations
		{
			get
			{
				if (orderReservations != null) return orderReservations;

				orderReservations = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(Reservation.ID)).ToList();
				if (orderReservations.Any()) return orderReservations;

				orderReservations = Service.OrderReferences.Select(x => DataMinerInterface.ResourceManager.GetReservationInstance(helpers, x)).ToList();
				return orderReservations;
			}
		}

		protected string OrderName
		{
			get
			{
				// Should be the name of the order that promoted the source to shared source
				return OrderReservations.SingleOrDefault(o => o.ID == FirstOrderReference)?.Name ?? OrderReservations.FirstOrDefault()?.Name ?? throw new OrderNotFoundException($"No orders found that use service {Reservation?.Name}");
			}
		}

		protected Guid FirstOrderReference
		{
			get
			{
				// Should be the first ID in the OrderReferences property
				if (Service != null)
				{
					if (!Service.OrderReferences.Any()) return Guid.Empty;
					return Service.OrderReferences.First();
				}
				else
				{
					if (!Reservation.GetOrderReferences().Any()) return Guid.Empty;
					return Reservation.GetOrderReferences().First();
				}
			}
		}

		protected string ShortDescription
		{
			get
			{
				var shortDescriptionProperty = Reservation.Properties.FirstOrDefault(x => x.Key.Equals(ServicePropertyNames.ShortDescription));
				return default(KeyValuePair<string, object>).Equals(shortDescriptionProperty) ? String.Empty : Convert.ToString(shortDescriptionProperty.Value);
			}
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ExternalSourceInfo other)) return false;
			return other.Reservation.ID.Equals(Reservation.ID);
		}

		public override int GetHashCode()
		{
			return Reservation.ID.GetHashCode();
		}
	}
}
