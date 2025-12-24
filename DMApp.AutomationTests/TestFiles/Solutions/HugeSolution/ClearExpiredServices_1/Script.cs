/*
****************************************************************************
*  Copyright (c) 2021,  Skyline Communications NV  All Rights Reserved.    *
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

dd/mm/2021	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

namespace ClearExpiredServices_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.Advanced;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ReportsAndDashboards;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.ServiceManager.Objects;
	using Skyline.DataMiner.Net.Ticketing;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	class Script : IDisposable
	{
		private Helpers helpers;
		private TicketingGatewayHelper ticketingHelper;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(Engine engine)
		{
			helpers = new Helpers(engine, Scripts.ClearExpiredServices);
			engine.Timeout = TimeSpan.FromHours(2);

			ticketingHelper = new TicketingGatewayHelper { HandleEventsAsync = false };
			ticketingHelper.RequestResponseEvent += (sender, args) => args.responseMessage = Engine.SLNet.SendSingleResponseMessage(args.requestMessage);

			ReportInfo reportInfo = new ReportInfo();
			reportInfo = RetrieveServicesToRemove(engine, reportInfo);
			reportInfo = RetrieveServiceDefinitionsToRemove(reportInfo);	

			if (reportInfo.ServicesToRemove.Any()) RemoveServices(reportInfo.ServicesToRemove);
			if (reportInfo.ReservationsToRemove.Any()) RemoveReservationInstances(reportInfo.ReservationsToRemove);
			if (reportInfo.TicketsToRemove.Any()) RemoveTickets(ticketingHelper, reportInfo.TicketsToRemove);
			//if (reportInfo.ServiceDefinitionsToRemove.Any()) RemoveServiceDefinitions(reportInfo.ServiceDefinitionsToRemove);

			GenerateReport(engine, reportInfo);

			Dispose();
		}

		private void Log(string nameOfMethod, string message)
		{
			helpers?.Log(nameof(Script), nameOfMethod, message);
		}

		private ReportInfo RetrieveServicesToRemove(Engine engine, ReportInfo reportInfo)
		{
			var dms = Engine.SLNetRaw.GetDms();

			// get the srm service view
			var srmServiceViews = dms.GetViews().Where(v => v.Name.Contains("Services")).ToList();
			if (!srmServiceViews.Any())
			{
				Log(nameof(RetrieveServicesToRemove), "No SRM service views found");
				return reportInfo;
			}

			reportInfo.ServicesToRemove = new List<DmsServiceId>();
			reportInfo.ReservationsToRemove = new List<ReservationInstance>();
			reportInfo.TicketsToRemove = new List<Ticket>();

			foreach (var service in dms.GetServices())
			{
				if (!service.Views.Overlaps(srmServiceViews))
				{
					// not an SRM service
					continue;
				}

				// find reservation instance with name of this service
				var reservationInstance = FindReservationInstance(service.Name);
				if (reservationInstance == null)
				{
					reportInfo.ServicesToRemove.Add(service.DmsServiceId);
					continue;
				}

				// check if it's an order or service
				if (IsOrderReservationInstance(reservationInstance))
				{
					// Remove the DM Service if the order is finished (this should have happened by the SRM solution)
					if (reservationInstance.End.AddHours(1) < DateTime.Now)
					{
						reportInfo.ServicesToRemove.Add(service.DmsServiceId);
					}

					// no need to remove the reservation instances of orders.
					continue;
				}

				// find service order reservation instances
				var orderReservationInstances = FindOrderReservationInstancesForService(reservationInstance);
				bool isUsedByOneOrMoreOrders = orderReservationInstances.Any();

				if (!isUsedByOneOrMoreOrders)
				{
					// remove service and service reservationInstance if there is no order reservation for it
					reportInfo.ReservationsToRemove.Add(reservationInstance);
					reportInfo.TicketsToRemove.AddRange(FindTicketsForService(ticketingHelper, reservationInstance));
					reportInfo.ServicesToRemove.Add(service.DmsServiceId);
				}
			}

			return reportInfo;
		}

		private ReportInfo RetrieveServiceDefinitionsToRemove(ReportInfo reportInfo)
		{
			reportInfo.ServiceDefinitionsToRemove = new List<ServiceDefinition>();

			var serviceDefinitions = DataMinerInterface.ServiceManager.GetServiceDefinitions(helpers, ServiceDefinitionExposers.Name.NotMatches("_.*").AND(ServiceDefinitionExposers.Name.NotMatches(".*[.]Default"))).ToArray();

			foreach (var serviceDefinition in serviceDefinitions)
			{
				try
				{
					var filter = ServiceReservationInstanceExposers.ServiceDefinitionID.Equal(serviceDefinition.ID);
					var reservationInstances = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, filter);

					if (reservationInstances == null)
					{
						reportInfo.ExtraMessages.Add($"Received null when requesting reservation instances using service definition {serviceDefinition.Name}");
						continue;
					}

					if (reservationInstances.Any()) continue; // the SD is being used

					reportInfo.ServiceDefinitionsToRemove.Add(serviceDefinition);
				}
				catch (Exception e)
				{
					reportInfo.ExtraMessages.Add($"Exception occurred while getting reservations for definition {serviceDefinition.ID}: {e}");
				}
			}

			return reportInfo;
		}

		private ReservationInstance FindReservationInstance(string name)
		{
			try
			{
				return DataMinerInterface.ResourceManager.GetReservationInstances(helpers, new ANDFilterElement<ReservationInstance>(ReservationInstanceExposers.Name.Contains(name))).FirstOrDefault();
			}
			catch (Exception)
			{
				// no need to log
			}

			return null;
		}

		private static bool IsOrderReservationInstance(ReservationInstance reservationInstance)
		{
			try
			{
				var virtualPlatformProperty = reservationInstance.Properties.FirstOrDefault(p => p.Key == "Virtual Platform");
				if (virtualPlatformProperty.Value == null || String.IsNullOrEmpty(Convert.ToString(virtualPlatformProperty.Value))) return true;

				return virtualPlatformProperty.Value.ToString() == "Order";
			}
			catch (Exception)
			{
				// no need to log
			}

			return true;
		}

		private List<ReservationInstance> FindOrderReservationInstancesForService(ReservationInstance serviceReservationInstance)
		{
			var orderReservationInstances = new List<ReservationInstance>();

			try
			{
				var contributingResourceFilter = ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(serviceReservationInstance.ID);

				orderReservationInstances = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, new ANDFilterElement<ReservationInstance>(contributingResourceFilter)).ToList();
			}
			catch (Exception ex)
			{
				Log(nameof(FindOrderReservationInstancesForService), $"Exception occurred while finding order reservations using contributing resource {serviceReservationInstance.ID}: {ex}");
			}

			return orderReservationInstances;
		}

		private static List<Ticket> FindTicketsForService(TicketingGatewayHelper helper, ReservationInstance serviceReservationInstance)
		{
			try
			{
				return helper.GetTickets(filter: TicketingExposers.CustomTicketFields.DictStringField("Service ID").Equal(serviceReservationInstance.ID.ToString())).ToList();
			}
			catch (Exception)
			{
				// no need to log
			}

			return new List<Ticket>();
		}

		private static void GenerateReport(Engine engine, ReportInfo reportInfo)
		{
			var message = new StringBuilder();

			message.AppendLine("Services to remove: ");
			if (!reportInfo.ServicesToRemove.Any()) message.Append("None<br>");
			foreach (var service in reportInfo.ServicesToRemove) message.AppendLine($"<br>{service.Value}");

			message.AppendLine("<br>");
			message.AppendLine("<br>Reservations to remove: ");
			if (!reportInfo.ReservationsToRemove.Any()) message.Append("None<br>");
			foreach (var reservationInstance in reportInfo.ReservationsToRemove) message.AppendLine($"<br>{reservationInstance.Name} [{reservationInstance.ID}]");

			message.AppendLine("<br>");
			message.AppendLine("<br>Tickets to remove: ");
			if (!reportInfo.TicketsToRemove.Any()) message.Append("None<br>");
			foreach (var ticket in reportInfo.TicketsToRemove) message.AppendLine($"<br>{ticket.ID}");

			message.AppendLine("<br>");
			message.AppendLine("<br>Service Definitions that are marked to be removed: ");
			if (!reportInfo.ServiceDefinitionsToRemove.Any()) message.Append("None<br>");
			foreach (var serviceDefinition in reportInfo.ServiceDefinitionsToRemove) message.AppendLine($"<br>{serviceDefinition.Name}");

			message.AppendLine("<br>");
			message.AppendLine("<br>Extra messages: ");
			foreach(var extraMessage in reportInfo.ExtraMessages) message.AppendLine($"<br>{extraMessage}");

			engine.SendEmail(new EmailOptions(message.ToString(), "YLE DEBUG - Clear Expired Services Report", "squad.deploy-the.pioneers@skyline.be"));
		}

		private void RemoveServices(List<DmsServiceId> services)
		{
			foreach (var serviceId in services)
			{
				try
				{
					helpers.Engine.SendSLNetMessage(new SetDataMinerInfoMessage
					{
						Uia1 = new UIA(new[] { (uint)serviceId.AgentId, (uint)serviceId.ServiceId }),
						What = 74
					});
				}
				catch (Exception e)
				{
					Log(nameof(RemoveServices), $"Exception removing service '{serviceId}': {e}");
				}
			}
		}

		private void RemoveReservationInstances(List<ReservationInstance> reservationInstances)
		{
			try
			{
				helpers.ResourceManager.RemoveReservationInstances(reservationInstances.ToArray());
			}
			catch (Exception e)
			{
				Log(nameof(RemoveReservationInstances), $"Exception removing reservations: {e}");
			}
		}

		private void RemoveTickets(TicketingGatewayHelper helper, List<Ticket> tickets)
		{
			if (tickets.Any())
			{
				try
				{
					if (!helper.RemoveTickets(out var error, tickets.ToArray()))
					{
						Log(nameof(RemoveTickets), $"Error removing tickets: {error}");
					}
				}
				catch (Exception e)
				{
					Log(nameof(RemoveTickets), $"Exception removing tickets: {e}");
				}
			}
		}

		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue && disposing)
			{
				helpers.Dispose();

				helpers.LockManager.ReleaseLocks();
			}

			disposedValue = true;
		}

		~Script()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		class ReportInfo
		{
			public List<DmsServiceId> ServicesToRemove { get; set; } = new List<DmsServiceId>();

			public List<ReservationInstance> ReservationsToRemove { get; set; } = new List<ReservationInstance>();

			public List<Ticket> TicketsToRemove { get; set; } = new List<Ticket>();

			public List<ServiceDefinition> ServiceDefinitionsToRemove { get; set; } = new List<ServiceDefinition>();

			public List<string> ExtraMessages { get; set; } = new List<string>();
		}
	}
}