namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reservations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Reservation;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using ServiceStatus = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status;


	public static class ReservationInstanceExtensions
	{
		public static string GetShortDescription(this ReservationInstance reservation)
		{
			return Convert.ToString(reservation.GetPropertyByName(ServicePropertyNames.ShortDescription));
		}

		public static HashSet<Guid> GetOrderReferences(this ReservationInstance reservation)
		{
			var orders = new HashSet<Guid>();

			if (!reservation.Properties.Dictionary.TryGetValue(ServicePropertyNames.OrderIdsPropertyName, out var orderIdsProperty))
			{
				return orders;
			}

			try
			{
				var orderIds = Convert.ToString(orderIdsProperty).Split(';').Select(id => Guid.Parse(id));
				foreach (var orderId in orderIds)
				{
					if (orderId == Guid.Empty) continue;

					orders.Add(orderId);
				}
			}
			catch (Exception)
			{
				return orders;
			}

			return orders;
		}

		public static EurovisionBookingDetails GetEurovisionBookingDetails(this ReservationInstance reservationInstance)
		{
			object eurovisionBookingDetailsPropertyValue;
			if (!reservationInstance.Properties.Dictionary.TryGetValue(ServicePropertyNames.EurovisionBookingDetailsPropertyName, out eurovisionBookingDetailsPropertyValue))
			{
				return new EurovisionBookingDetails();
			}

			var eurovisionBookingDetails = Convert.ToString(eurovisionBookingDetailsPropertyValue);
			if (string.IsNullOrEmpty(eurovisionBookingDetails))
			{
				return new EurovisionBookingDetails();
			}

			return JsonConvert.DeserializeObject<EurovisionBookingDetails>(eurovisionBookingDetails);
		}

		public static BackupType GetServiceLevel(this ReservationInstance reservationInstance)
		{
			if (!reservationInstance.Properties.Dictionary.TryGetValue(ServicePropertyNames.ServiceLevelPropertyName, out var serviceLevel))
			{
				return BackupType.None;
			}

			return (BackupType)Convert.ToInt32(serviceLevel);
		}

		public static IntegrationType GetIntegrationType(this ReservationInstance reservationInstance)
		{
			object integrationType;
			if (!reservationInstance.Properties.Dictionary.TryGetValue(ServicePropertyNames.IntegrationTypePropertyName, out integrationType))
			{
				return IntegrationType.None;
			}

			return EnumExtensions.GetEnumValueFromDescription<IntegrationType>(Convert.ToString(integrationType));
		}

		public static bool GetBooleanProperty(this ReservationInstance reservation, string propertyName)
		{
			string propertyValue = GetStringProperty(reservation, propertyName);
			return !string.IsNullOrWhiteSpace(propertyValue) && Convert.ToBoolean(propertyValue);
		}

		public static string GetStringProperty(this ReservationInstance reservation, string propertyName)
		{
			if (reservation == null) throw new ArgumentNullException(nameof(reservation));
			if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));

			return reservation.Properties.Dictionary.TryGetValue(propertyName, out object propertyValue) ? Convert.ToString(propertyValue) : String.Empty;
		}

		public static RecordingConfiguration GetRecordingConfiguration(this ReservationInstance reservationInstance)
		{
			if (!reservationInstance.Properties.Dictionary.TryGetValue(ServicePropertyNames.RecordingConfigurationPropertyName, out var recordingConfigurationPropertyValue))
			{
				return new RecordingConfiguration();
			}

			try
			{
				return RecordingConfiguration.Deserialize(Convert.ToString(recordingConfigurationPropertyValue));
			}
			catch (Exception)
			{
				return new RecordingConfiguration();
			}
		}

		public static ServiceStatus GetServiceStatus(this ReservationInstance reservationInstance)
		{
			object status;
			if (!reservationInstance.Properties.Dictionary.TryGetValue(ServicePropertyNames.Status, out status))
			{
				throw new ServiceReservationPropertyNotFoundException(ServicePropertyNames.Status, reservationInstance.Name);
			}

			return EnumExtensions.GetEnumValueFromDescription<ServiceStatus>(Convert.ToString(status));
		}

		public static Guid GetLinkedServiceId(this ReservationInstance reservationInstance)
		{
			var linkedServiceIdProperty = reservationInstance.Properties.FirstOrDefault(p => string.Equals(p.Key, ServicePropertyNames.LinkedServiceIdPropertyName, StringComparison.InvariantCultureIgnoreCase));
			if (linkedServiceIdProperty.Equals(default(KeyValuePair<string, object>)) || linkedServiceIdProperty.Value == null) return Guid.Empty;

			Guid linkedServiceId;
			if (!Guid.TryParse(Convert.ToString(linkedServiceIdProperty.Value), out linkedServiceId)) return Guid.Empty;

			return linkedServiceId;
		}

		public static T GetProfileParameterValue<T>(this ReservationInstance reservation, string name, Helpers helpers)
		{
			try
			{
				var functionData = reservation.GetFunctionData();
				foreach (var function in functionData)
				{
					foreach (var parameter in function.Parameters)
					{
						var profileParameterInfo = helpers.ProfileManager.GetProfileParameter(parameter.Id);
						if (profileParameterInfo?.Name != name) continue;

						return (T)Convert.ChangeType(parameter.Value, typeof(T));
					}
				}
			}
			catch (Exception e)
			{
				helpers.Log(
					nameof(ReservationInstanceExtensions),
					nameof(GetProfileParameterValue),
					$"Exception retrieving profile parameter {name} from reservation {reservation.ID}: {e}");
			}

			return default(T);
		}

		public static ReservationInstance ChangeStateToConfirmedWithRetry(this ReservationInstance reservation, Helpers helpers, BookingManager bookingManager, int retryAttempt = 0)
		{
			if (reservation.Status == Net.Messages.ReservationStatus.Confirmed)
			{
				helpers.Log(nameof(ReservationInstanceExtensions), nameof(ChangeStateToConfirmedWithRetry), $"Reservation {reservation.Name} has already been set to confirmed", reservation.Name);
				return reservation;
			}

			if (retryAttempt > 4)
			{
				helpers.Log(nameof(ReservationInstanceExtensions), nameof(ChangeStateToConfirmedWithRetry), "Max retries of 4 is reached", reservation.Name);
				return reservation;
			}

			try
			{
				helpers.Log(nameof(ReservationInstanceExtensions), nameof(ChangeStateToConfirmedWithRetry), $"Changing SRM state to confirmed, attempt {retryAttempt}", reservation.Name);

				reservation = DataMinerInterface.BookingManager.ChangeStateToConfirmed(helpers, bookingManager, reservation); // required to let the service transition to different states

				helpers.Log(nameof(ReservationInstanceExtensions), nameof(ChangeStateToConfirmedWithRetry), $"Changing SRM state to confirmed completed", reservation.Name);
			}
			catch (Exception ex)
			{
				helpers.Log(nameof(ReservationInstanceExtensions), nameof(ChangeStateToConfirmedWithRetry), $"Changing SRM state of reservation to Confirmed failed: {ex}", reservation.Name);
				Thread.Sleep(350);
				return reservation.ChangeStateToConfirmedWithRetry(helpers, bookingManager, ++retryAttempt);
			}

			return reservation;
		}

	}
}
