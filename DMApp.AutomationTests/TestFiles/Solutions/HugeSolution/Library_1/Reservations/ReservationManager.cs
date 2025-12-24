namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reservations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class ReservationManager : HelpedObject
	{
		public ReservationManager(Helpers helpers) : base(helpers)
		{

		}

		public ReservationInstance GetReservation(Guid id)
		{
			return DataMinerInterface.ResourceManager.GetReservationInstance(helpers, id);
		}

		public ReservationInstance GetReservation(string name)
		{
			return DataMinerInterface.ResourceManager.GetReservationInstances(helpers, new ANDFilterElement<ReservationInstance>(ReservationInstanceExposers.Name.Equal(name))).FirstOrDefault();
		}

		public ReservationInstance UpdateSecurityViewIds(ReservationInstance reservation, IEnumerable<int> securityViewIds)
		{
			if (reservation == null) throw new ArgumentNullException(nameof(reservation));
			if (securityViewIds == null) throw new ArgumentNullException(nameof(securityViewIds));

			Log(nameof(UpdateSecurityViewIds), $"Security view IDs to set: '{string.Join(";", securityViewIds)}'", reservation.Name);
			Log(nameof(UpdateSecurityViewIds), $"Current reservation security view ids: '{string.Join(";", reservation.SecurityViewIDs)}'", reservation.Name);

			var existingSecurityViewIds = new HashSet<int>(reservation.SecurityViewIDs);
			if (!existingSecurityViewIds.SetEquals(securityViewIds))
			{
				Log(nameof(UpdateSecurityViewIds), $"Security view ids will be updated", reservation.Name);

				reservation.SecurityViewIDs = securityViewIds.ToList();
				var updatedReservation = DataMinerInterface.ResourceManager.AddOrUpdateReservationInstances(helpers, reservation).FirstOrDefault();

				Log(nameof(UpdateSecurityViewIds), $"Updated reservation security view ids : '{string.Join(";", updatedReservation?.SecurityViewIDs ?? reservation.SecurityViewIDs)}'", reservation.Name);

				return updatedReservation ?? reservation;
			}

			return reservation;
		}

		/// <summary>
		/// Updates the custom properties on the order reservation.
		/// </summary>
		/// <param name="customProperties">A dictionary with property names as key and property values as value.</param>
		/// <param name="reservation"></param>
		public bool TryUpdateCustomProperties(ReservationInstance reservation, Dictionary<string, object> customProperties)
		{
			ArgumentNullCheck.ThrowIfNull(customProperties, nameof(customProperties));
			ArgumentNullCheck.ThrowIfNull(reservation, nameof(reservation));

			try
			{
				// To avoid that null values inside the custom properties dictionary throw a 'value is not serializable to JSON' exception.
				var keysOfCustomProperties = new List<string>(customProperties.Keys);
				foreach (var key in keysOfCustomProperties)
				{
					if (customProperties[key] == null) customProperties[key] = String.Empty;
				}

				if (customProperties.Any())
				{
					helpers.Log(nameof(Order), nameof(TryUpdateCustomProperties), $"Update custom properties {string.Join(";", customProperties.Select(cp => $"{cp.Key}={cp.Value}"))} on reservation {reservation.Name} ({reservation.ID})", reservation.Name);

					DataMinerInterface.ReservationInstance.UpdateServiceReservationProperties(helpers, reservation, customProperties);
				}
				else
				{
					helpers.Log(nameof(Order), nameof(TryUpdateCustomProperties), $"No custom properties to update on order reservation", reservation.Name);
				}
			}
			catch (Exception e)
			{
				Log(nameof(TryUpdateCustomProperties), $"Exception updating order reservation properties: {string.Join(",", customProperties.Select(x => $"{x.Key}:{x.Value}"))} \n {e}", reservation.Name);
				return false;
			}

			return true;
		}
	}
}
