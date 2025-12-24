/*
****************************************************************************
*  Copyright (c) 2022,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

dd/mm/2022	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

namespace UpdateServiceStatus_1
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Automation;
	using Skyline.DataMiner.Library.Solutions.SRM.LifecycleServiceOrchestration;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.Events;
	using Skyline.DataMiner.Utils.YLE.Integrations;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private Helpers helpers;

		public void Run(Engine engine)
		{
			Initialize(engine);

			try
			{
				var serviceReservationId = Guid.Parse(engine.GetScriptParam("ReservationGuid").Value);

				var service = helpers.ServiceManager.GetService(serviceReservationId) ?? throw new ServiceNotFoundException(serviceReservationId);

				var orderReservationId = service.OrderReferences.First();

				helpers.AddOrderReferencesForLogging(orderReservationId);

				var action = engine.GetScriptParam("Action").Value.ToLower();

				if (action.Contains("updateuiproperties"))
				{
					HandleUpdateUiPropertiesAction(service);
				}
				else if (action.Contains("extend"))
				{
					//HandleExtendAction(engine, serviceManager, service);
				}
				else
				{
					var enhancedAction = new LsoEnhancedAction(engine.GetScriptParamValue<string>("Action"));

					bool triggeredByPreRoll = enhancedAction.Event == SrmEvent.START_BOOKING_WITH_PREROLL;
					if (service.PreRoll == TimeSpan.Zero && triggeredByPreRoll) return; // No need to update the service cause the start event will immediately follow afterwards.

					HandleServiceStartStopActions(service, orderReservationId, enhancedAction);
				}
			}
			catch (Exception e)
			{
				Log(nameof(Run), $"Exception handling service action: {e}");
			}
			finally
			{
				Dispose();
			}
		}

		private void Initialize(Engine engine)
		{
			engine.Timeout = TimeSpan.FromHours(1);
			engine.SetFlag(RunTimeFlags.NoKeyCaching);

			this.helpers = new Helpers(engine, Scripts.UpdateServiceStatus);
		}

		private void HandleUpdateUiPropertiesAction(Service service)
		{
			// only applicable for Satellite RX. Script is triggered in Reception.Satellite Service Definition action
			if (service.OrderReferences == null || !service.OrderReferences.Any()) return;

			foreach (var orderGuid in service.OrderReferences)
			{
				helpers.OrderManager.UpdateUIProperties(orderGuid);
			}
		}

		/*
	private void HandleExtendAction(Engine engine, ServiceManager serviceManager, Service service)
	{
		if (!AreServiceExtensionConditionsMet(service, serviceManager)) return;

		var order = helpers.OrderManager.GetOrder(service.OrderReferences.First());

		var serviceToExtend = order.AllServices.FirstOrDefault(x => x.Id == service.Id);
		if (serviceToExtend == null) return;

		serviceToExtend.End += TimeSpan.FromMinutes(10);
		serviceManager.TryChangeServiceTime(serviceToExtend);

		order.End = order.AllServices.Select(x => x.End).Max();
		helpers.OrderManager.ChangeOrderTime(order);

		Service sourceService = order.Sources.FirstOrDefault(x => x.BackupType == BackupType.None);
		if (sourceService != null && serviceToExtend.End > sourceService.End)
		{
			NotificationManager.SendMailToMcrUsers(engine, "Plasma Service extended beyond Source", String.Format("Order {2}: extension of Plasma Service {0} due to not receiving an end trigger from Pebble Beach will cause it to run longer than Source Service {1}", service.Name, sourceService.Name, order.Name));
		}
	}
	*/

		private bool AreServiceExtensionConditionsMet(Service service, ServiceManager serviceManager)
		{
			try
			{
				// only non-reception plasma services should be extended
				if (service.IntegrationType != IntegrationType.Plasma) return false;

				if (service.End < DateTime.UtcNow + TimeSpan.FromMinutes(2))
				{
					// service has been ended by HandleIntegrationUpdate and should not be extended again
					return false;
				}

				var newServiceEndWithPostRoll = service.EndWithPostRoll + TimeSpan.FromMinutes(10);
				if (service.Functions != null && service.Functions.Any())
				{
					foreach (var function in service.Functions)
					{
						var servicesUsingSameResource = serviceManager.GetServicesWithSpecificResourceAndWithinTimeSpan(function.Resource, service.EndWithPostRoll, newServiceEndWithPostRoll).Where(x => x.Id != service.Id);
						if (servicesUsingSameResource.Any())
						{
							NotificationManager.SendMailToMcrUsers(helpers, "Unable to Extend Plasma Service " + service.Name, $"Extension of Plasma Service {service.Name} due to not receiving an end trigger from Pebble Beach is not possible because resource {function.Resource.Name} is already being used by services {String.Join(", ", servicesUsingSameResource)} from {service.EndWithPostRoll} until {newServiceEndWithPostRoll}");
							return false;
						}
					}
				}
				else
				{
					Log(nameof(AreServiceExtensionConditionsMet), "Unable to check resource availability");
				}

				return true;
			}
			catch (Exception e)
			{
				Log(nameof(AreServiceExtensionConditionsMet), "Unable to determine if Service Extension is possible, " + e);
				return false;
			}
		}

		private void GenerateAdditionalUserTasks(UserTaskManager userTaskManager, Service service, Order linkedOrder)
		{
			// in case of a recording service
			// additional user tasks need to be generated when the recording service transitions to post roll

			// in case of a satellite transmission service
			// additional user tasks need to be generated when the satellite transmission service transitions to start time.

			LogMethodStart(nameof(GenerateAdditionalUserTasks), out var stopwatch);

			try
			{
				Log(nameof(GenerateAdditionalUserTasks), $"Current user task objects in service object: {string.Join(";", service.UserTasks.Select(u => $"{u.Name}({u.ID})={u.Status}"))}");

				userTaskManager.AddOrUpdateUserTasks(service, linkedOrder);
			}
			catch (Exception e)
			{
				Log(nameof(GenerateAdditionalUserTasks), "Error adding or updating user tasks: " + e);
			}

			LogMethodCompleted(nameof(GenerateAdditionalUserTasks), stopwatch);
		}

		private void HandleServiceStartStopActions(Service service, Guid? orderReservationId, LsoEnhancedAction action)
		{
			LogMethodStart(nameof(HandleServiceStartStopActions), out var stopwatch, service.Name);

			var order = helpers.OrderManager.GetOrder(orderReservationId.Value) ?? throw new OrderNotFoundException(orderReservationId.Value);

			// Replace service in order object.
			// Previously we had issues with StartNow orders where the UpdateTicketStatus script was still busy starting services and lastly updating the ServiceConfiguration.
			// As the services were starting, the HandleServiceAction script was already triggered before the ServiceConfiguration was updated.
			// As the Service timing is retrieved from the ServiceConfiguration, the wrong timing was used to determine the Service status.
			ReplaceService(order, service);

			Log(nameof(HandleServiceStartStopActions), $"Order: {order.Name}|Status: {order.Status}");

			bool triggeredByPostRollStart = action.Event == SrmEvent.STOP;
			bool triggeredByPostRollEnd = action.Event == SrmEvent.STOP_BOOKING_WITH_POSTROLL;
			bool recordingIsBeingStopped = (triggeredByPostRollStart || triggeredByPostRollEnd) && service.Definition.VirtualPlatform == VirtualPlatform.Recording;
			bool satelliteTransmissionIsBeingStarted = action.Event == SrmEvent.START && service.Definition?.VirtualPlatform == VirtualPlatform.TransmissionSatellite;

			if (recordingIsBeingStopped || satelliteTransmissionIsBeingStarted)
			{
				Log("HandleServiceStartStopActions", "Creating additional user tasks");
				GenerateAdditionalUserTasks(helpers.UserTaskManager, service, order);
			}

			if (triggeredByPostRollStart && service.PostRoll == TimeSpan.Zero)
			{
				LogMethodCompleted(nameof(HandleServiceStartStopActions), stopwatch); // No need to update service status as the post roll end event will immediately follow afterwards.
				return;
			}

			if (triggeredByPostRollEnd && service.Definition.VirtualPlatform == VirtualPlatform.ReceptionSatellite)
			{
				// Clearing order name in Visio of main element to give not in use indication (Event Name property is used as this name is used in overall Visio)
				service.UpdateFunctionsMainElementOrderNameProperty(helpers, String.Empty);
			}

			Log("HandleServiceStartStopActions", "Updating service status");
			service.TryUpdateStatus(helpers, order, action, updateOrderStatus: false);

			LogMethodCompleted(nameof(HandleServiceStartStopActions), stopwatch);
		}

		/// <summary>
		/// Replaces the matching service in the order with the provided service object.
		/// Matching services are determined 
		/// </summary>
		/// <param name="order">Order to be updated.</param>
		/// <param name="service">Service to be placed in the order.</param>
		private static void ReplaceService(Order order, Service service)
		{
			Service serviceToReplace = order.AllServices.FirstOrDefault(x => x.Id == service.Id);
			if (serviceToReplace == null) return;

			service.SetChildren(serviceToReplace.Children);

			Service parentService = order.AllServices.FirstOrDefault(x => x.Children.Contains(serviceToReplace));
			if (parentService != null)
			{
				parentService.Children.Remove(serviceToReplace);
				parentService.Children.Add(service);
			}
			else
			{
				order.Sources.Remove(serviceToReplace);
				order.Sources.Add(service);
			}
		}

		private void Log(string nameOfMethod, string message, string nameOfObject = null)
		{
			helpers?.Log(nameof(Script), nameOfMethod, message, nameOfObject);
		}

		private void LogMethodStart(string nameOfMethod, out Stopwatch stopwatch, string nameOfObject = null)
		{
			helpers.LogMethodStart(nameof(Script), nameOfMethod, out stopwatch, nameOfObject);
		}

		private void LogMethodCompleted(string nameOfMethod, Stopwatch stopwatch)
		{
			helpers.LogMethodCompleted(nameof(Script), nameOfMethod, null, stopwatch);
		}

		#region IDisposable Support
		private bool disposedValue; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					helpers.Dispose();
				}

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		~Script()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(false);
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);

			// TODO: uncomment the following line if the finalizer is overridden above.
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}