namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface
{
	using System.Collections.Generic;
	using System.Reflection;
	using Library.Solutions.SRM;
	using Library.Solutions.SRM.Model.AssignProfilesAndResources;
	using Net.ResourceManager.Objects;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Reservation;

	public static partial class DataMinerInterface
	{
		public static class ReservationInstance
		{
			[WrappedMethod("ReservationInstance", "UpdateServiceReservationProperties", 60000)]
			public static void UpdateServiceReservationProperties(Helpers helpers, Net.ResourceManager.Objects.ReservationInstance reservationInstance, Dictionary<string, object> properties)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, reservationInstance.Name);

				reservationInstance.UpdateServiceReservationProperties(false, properties);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);
			}

			[WrappedMethod("ReservationInstance", "AssignResources")]
			public static ServiceReservationInstance AssignResources(Helpers helpers, ServiceReservationInstance reservationInstance, params AssignResourceRequest[] requests)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch, reservationInstance.Name);

				var result = reservationInstance.AssignResources((Automation.Engine) helpers.Engine, requests);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}
		}
	}
}