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

namespace ForceDeleteOrder_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.Advanced;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Ticketing;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	class Script : IDisposable
	{
		private Helpers helpers;
		private bool disposedValue;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(Engine engine)
		{
			helpers = new Helpers(engine, Scripts.ForceDeleteOrder);

			var orderName = engine.GetScriptParam("Order Name").Value;
			if (String.IsNullOrEmpty(orderName))
			{
				engine.Log("(ForceDeleteOrder) No valid order name provided!");
				return;
			}

			var orderReservationInstance = GetOrderReservationInstance(helpers, orderName);
			RemoveOrderAndServices(helpers, orderReservationInstance);	
			RemoveOrderFromEvent(engine, orderReservationInstance);
			RemoveOrderFromOrderManager(engine, orderReservationInstance);
		}

		private static ServiceReservationInstance GetOrderReservationInstance(Helpers helpers, string name)
		{
			try
			{
				var filter = new ANDFilterElement<ReservationInstance>(ReservationInstanceExposers.Name.Equal(name));
				var orderReservationInstance = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, filter).FirstOrDefault() as ServiceReservationInstance;
				if (orderReservationInstance == null)
				{
					helpers.Engine.ExitFail(String.Format("(GetOrderReservationInstance) Order '{0}' reservation instance not found", name));
					return null;
				}

				return orderReservationInstance;
			}
			catch (Exception e)
			{
				helpers.Engine.ExitFail(String.Format("(GetOrderReservationInstance) Exception retrieving order '{0}' reservation instance: {1}", name, e));
				return null;
			}
		}

		private static void RemoveOrderAndServices(Helpers helpers, ServiceReservationInstance orderReservationInstance)
		{
			var ticketingHelper = new TicketingGatewayHelper { HandleEventsAsync = false };
			ticketingHelper.RequestResponseEvent += (sender, args) => args.responseMessage = Engine.SLNet.SendSingleResponseMessage(args.requestMessage);

			var reservationInstancesToRemove = new List<ReservationInstance> { orderReservationInstance };

			var servicesToRemove = new List<ServiceID>();
			if (orderReservationInstance.ServiceID != null) servicesToRemove.Add(orderReservationInstance.ServiceID);

			var ticketsToRemove = new List<Ticket>();

			foreach (var resourceUsage in orderReservationInstance.ResourcesInReservationInstance)
			{
				try
				{
					var resourceReservationInstance = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, resourceUsage.GUID) as ServiceReservationInstance;
					if (resourceReservationInstance == null)
					{
						helpers.Log(nameof(Script), nameof(RemoveOrderAndServices), $"Service '{resourceUsage.GUID}' reservation instance not found");
						continue;
					}

					reservationInstancesToRemove.Add(resourceReservationInstance);
					if (resourceReservationInstance.ServiceID != null) servicesToRemove.Add(resourceReservationInstance.ServiceID);

					try
					{
						var userTaskTickets = ticketingHelper.GetTickets(filter: TicketingExposers.CustomTicketFields.DictStringField("Service ID").Equal(resourceUsage.GUID.ToString()));
						if (userTaskTickets.Any()) ticketsToRemove.AddRange(userTaskTickets);
					}
					catch (Exception e)
					{
						helpers.Log(nameof(Script), nameof(RemoveOrderAndServices), $"Exception retrieving user tasks for service '{resourceUsage.GUID}': {e}");
					}
				}
				catch (Exception e)
				{
					helpers.Log(nameof(Script), nameof(RemoveOrderAndServices), $"Exception retrieving order '{resourceUsage.GUID}' reservation instance: {e}");
				}
			}

			RemoveReservationInstances(helpers, reservationInstancesToRemove);
			RemoveServices((Engine)helpers.Engine, servicesToRemove);
			RemoveTickets((Engine)helpers.Engine, ticketingHelper, ticketsToRemove);
		}

		private static void RemoveReservationInstances(Helpers helpers, List<ReservationInstance> reservationInstances)
		{
			try
			{
				DataMinerInterface.ResourceManager.RemoveReservationInstances(helpers, reservationInstances.ToArray());
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(RemoveReservationInstances),$"Exception removing reservations: {e}");
			}
		}

		private static void RemoveServices(Engine engine, List<ServiceID> services)
		{
			foreach (var serviceId in services)
			{
				try
				{
					engine.SendSLNetMessage(new SetDataMinerInfoMessage
					{
						Uia1 = new UIA(new[] { (uint)serviceId.DataMinerID, (uint)serviceId.SID }),
						What = 74
					});
				}
				catch (Exception e)
				{
					engine.Log(String.Format("(RemoveServices) Exception removing service '{0}': {1}", serviceId, e));
				}
			}
		}

		private static void RemoveTickets(Engine engine, TicketingGatewayHelper ticketingHelper, List<Ticket> tickets)
		{
			if (tickets.Any())
			{
				try
				{
					if (!ticketingHelper.RemoveTickets(out var error, tickets.ToArray()))
					{
						engine.Log(String.Format("(RemoveTickets) Error removing tickets: {0}", error));
					}
				}
				catch (Exception e)
				{
					engine.Log(String.Format("(RemoveTickets) Exception removing tickets: {0}", e));
				}
			}
		}

		private static void RemoveOrderFromEvent(Engine engine, ReservationInstance orderReservationInstance)
		{
			var eventIdPropertyValue = orderReservationInstance.Properties.FirstOrDefault(p => p.Key == "EventId").Value;
			if (eventIdPropertyValue == null)
			{
				engine.Log("(RemoveOrderFromEvent) Order EventId property is null");
				return;
			}

			if (!Guid.TryParse(Convert.ToString(eventIdPropertyValue), out var eventId))
			{
				engine.Log(String.Format("(RemoveOrderFromEvent) Order EventId property is not a valid Guid: {0}", eventIdPropertyValue));
				return;
			}

			var helper = new JobManagerHelper(m => engine.SendSLNetMessages(m));

			Job job = null;
			try
			{
				job = helper.Jobs.Read(JobExposers.ID.Equal(eventId)).FirstOrDefault();
				if (job == null)
				{
					engine.Log(String.Format("(RemoveOrderFromEvent) Job {0} not found", eventId));
					return;
				}
			}
			catch (Exception e)
			{
				engine.Log(String.Format("(RemoveOrderFromEvent) Exception retrieving job '{0}': {1}", eventId, e));
				return;
			}

			try
			{
				foreach (var section in job.Sections)
				{
					foreach (var fieldValue in section.FieldValues)
					{
						bool sectionContainsOrderId = fieldValue.Value.Type == typeof(Guid);
						if (sectionContainsOrderId && (Guid)fieldValue.Value.Value == orderReservationInstance.ID)
						{
							job.Sections.Remove(section);
							helper.Jobs.Update(job);
							return;
						}
					}
				}
			}
			catch (Exception e)
			{
				engine.Log(String.Format("(RemoveOrderFromEvent) Exception removing order '{0}' from job '{1}': {2}", orderReservationInstance.ID, eventId, e));
			}
		}

		private static void RemoveOrderFromOrderManager(Engine engine, ReservationInstance orderReservationInstance)
		{
			try
			{
				var orderManagerElement = engine.FindElementsByProtocol("Finnish Broadcasting Company Order Manager").FirstOrDefault() ?? throw new NotFoundException("Unable to find order manager element");
				orderManagerElement.SetParameterByPrimaryKey(2006, orderReservationInstance.ID.ToString(), "Delete");
			}
			catch (Exception e)
			{
				engine.Log(String.Format("(RemoveOrderFromOrderManager) Exception removing order '{0}' from order manager element: {1}", orderReservationInstance.ID, e));
			}
		}

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

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}