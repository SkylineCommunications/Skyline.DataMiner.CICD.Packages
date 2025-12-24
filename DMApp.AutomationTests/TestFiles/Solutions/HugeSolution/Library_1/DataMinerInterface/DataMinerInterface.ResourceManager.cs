namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Net.Messages.SLDataGateway;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public static partial class DataMinerInterface
	{
		public static class ResourceManager
		{
			[WrappedMethod("ResourceManager", "GetReservationInstance", 5000)]
			public static Net.ResourceManager.Objects.ReservationInstance GetReservationInstance(Helpers helpers, Guid id)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var reservation = SrmManagers.ResourceManager.GetReservationInstance(id);

				if (reservation != null)
				{
					//Debuglogging for DCP217913
					Log(helpers, MethodBase.GetCurrentMethod(), $"Reservation {reservation.Name} ({reservation.ID}) has security view IDs '{String.Join(", ", reservation.SecurityViewIDs)}'");
					if (!reservation.SecurityViewIDs.Any()) NotificationManager.SendMailTo(helpers, $"Missing Security View IDs", $"Reservation {reservation.Name} ({reservation.ID}) has security view IDs '{String.Join(", ", reservation.SecurityViewIDs)}'", "victor.scherpereel@skyline.be");
				}
				
				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return reservation;
			}

			[WrappedMethod("ResourceManager", "GetReservationInstances")]
			public static IEnumerable<Net.ResourceManager.Objects.ReservationInstance> GetReservationInstances(Helpers helpers, FilterElement<Net.ResourceManager.Objects.ReservationInstance> filter)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var reservations = SrmManagers.ResourceManager.GetReservationInstances(filter);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return reservations;
			}

			[WrappedMethod("ResourceManager", "GetReservationInstancesByProperty")]
			public static IEnumerable<Net.ResourceManager.Objects.ReservationInstance> GetReservationInstancesByProperty(Helpers helpers, string propertyName, object propertyValue)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var reservations = SrmManagers.ResourceManager.GetReservationInstancesByProperty(propertyName, propertyValue);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return reservations;
			}

			[WrappedMethod("ResourceManager", "GetReservationInstancesByServiceDefinition")]
			public static IEnumerable<Net.ResourceManager.Objects.ReservationInstance> GetReservationInstancesByServiceDefinition(Helpers helpers, Net.ServiceManager.Objects.ServiceDefinition serviceDefinition)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var reservations = SrmManagers.ResourceManager.GetReservationInstancesByServiceDefinition(serviceDefinition);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return reservations;
			}

			[WrappedMethod("ResourceManager", "AddOrUpdateReservationInstances")]
			public static Net.ResourceManager.Objects.ReservationInstance[] AddOrUpdateReservationInstances(Helpers helpers, params Net.ResourceManager.Objects.ReservationInstance[] reservationInstances)
			{
				using (StartPerformanceLogging(helpers))
				{
					var resourceManager = SrmManagers.ResourceManager;
					var reservations = resourceManager.AddOrUpdateReservationInstances(reservationInstances);
					var traceData = resourceManager.GetTraceDataLastCall();

					helpers.Log(nameof(DataMinerInterface), nameof(AddOrUpdateReservationInstances), $"TraceData: {traceData.ToString()}");

					if (!traceData.HasSucceeded())
					{
						throw new InvalidOperationException($"Unable to update reservations {String.Join(", ", reservationInstances.Select(x => x.ID))} due to: {String.Join(", ", traceData.ErrorData)}");
					}

					return reservations;
				}		
			}

			[WrappedMethod("ResourceManager", "RemoveReservationInstances")]
			public static Net.ResourceManager.Objects.ReservationInstance[] RemoveReservationInstances(Helpers helpers, params Net.ResourceManager.Objects.ReservationInstance[] reservationInstances)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var reservations = SrmManagers.ResourceManager.RemoveReservationInstances(reservationInstances);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return reservations;
			}

			[WrappedMethod("ResourceManager", "GetEligibleResources")]
			public static EligibleResourceResult GetEligibleResources(Helpers helpers, EligibleResourceContext context)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ResourceManager.GetEligibleResources(context);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("ResourceManager", "GetEligibleResources")]
			public static List<EligibleResourceResult> GetEligibleResources(Helpers helpers, List<EligibleResourceContext> contexts)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ResourceManager.GetEligibleResources(contexts);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("ResourceManager", "GetResource")]
			public static Net.Messages.Resource GetResource(Helpers helpers, Guid resourceId)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);
				
				var result = SrmManagers.ResourceManager.GetResource(resourceId);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("ResourceManager", "GetResources")]
			public static IEnumerable<Net.Messages.Resource> GetResources(Helpers helpers, params Net.Messages.Resource[] filters)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ResourceManager.GetResources(filters);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("ResourceManager", "GetResources")]
			public static IEnumerable<Net.Messages.Resource> GetResources(Helpers helpers, FilterElement<Net.Messages.Resource> filter)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ResourceManager.GetResources(filter);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("ResourceManager", "AddOrUpdateResources")]
			public static IEnumerable<Net.Messages.Resource> AddOrUpdateResources(Helpers helpers, IEnumerable<Net.Messages.Resource> resources)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ResourceManager.AddOrUpdateResources(resources.ToArray());

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("ResourceManager", "GetResourcePool")]
			public static Net.Messages.ResourcePool GetResourcePool(Helpers helpers, Guid id)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ResourceManager.GetResourcePool(id);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("ResourceManager", "GetResourcePools")]
			public static Net.Messages.ResourcePool[] GetResourcePools(Helpers helpers, params Net.Messages.ResourcePool[] filters)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ResourceManager.GetResourcePools(filters);

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}

			[WrappedMethod("ResourceManager", "GetTraceDataLastCall")]
			public static Net.TraceData GetTraceDataLastCall(Helpers helpers)
			{
				LogMethodStart(helpers, MethodBase.GetCurrentMethod(), out var stopwatch);

				var result = SrmManagers.ResourceManager.GetTraceDataLastCall();

				LogMethodCompleted(helpers, MethodBase.GetCurrentMethod(), stopwatch);

				return result;
			}
		}
	}
}