namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public interface IOrderManager
	{
		Order GetOrder(Guid id, bool forceServiceReservationsToOverwriteServiceConfig = false, bool skipGettingEvent = false);

		Order GetOrder(ReservationInstance reservation, bool forceServiceReservationsToOverwriteServiceConfig = false, bool skipGettingEvent = false);

		LiteOrder GetLiteOrder(Guid id, bool skipGettingEvent = false);

        ReservationInstance GetPlasmaReservationInstance(string programId, string plasmaId);

		Order GetPlasmaOrder(string programId, string plasmaId);

        ReservationInstance GetFeenixReservationInstance(string yleId);

		Order GetFeenixOrder(string yleId);

		List<Order> GetEurovisionOrders(string transmissionNumber);

        List<Order> GetManualEurovisionOrders(string workOrderId);

		IEnumerable<ReservationInstance> GetAllFutureAndOngoingVideoReservations();

		IEnumerable<Task> DeleteOrder(Order order, List<Guid> serviceIdsToKeep = null);
	}
}
