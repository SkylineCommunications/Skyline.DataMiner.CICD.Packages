/*
****************************************************************************
*  Copyright (c) 2020,  Skyline Communications NV  All Rights Reserved.    *
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

dd/mm/2020	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

namespace ClearReservations_2
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.Advanced;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.ServiceManager.Objects;
	using Skyline.DataMiner.Net.Ticketing;
	using Skyline.DataMiner.Net.Ticketing.Helpers;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	class Script
	{
		private static readonly string[] contributingResourcePoolNames = new[] { "Audio Processing", "Destination Service", "Graphics Processing Service", "Reception", "Recording Service", "Routing", "Transmission", "Video Processing Service" };

		private static readonly string[] serviceNamesToKeep = new[] { "YleX" };

		private const string userTasksTicketDomainName = "User Tasks";
		//private const string ingestExportTicketDomainName = "Ingest/Export";
		private const string notesTicketDomainName = "Notes";

		private bool clearNonLive;

		private const string IngestExportFkTicketField = "Ingest Export FK";
		private const string ServiceIdTicketField = "Service ID";

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			if (engine.GetScriptParam(1).Value != "yes") return;

			clearNonLive = bool.TryParse(engine.GetScriptParam(3).Value, out bool parsedClearNonLive) && parsedClearNonLive;

			engine.Timeout = TimeSpan.FromHours(5);

			ClearSrmData(engine);
			ClearServices(engine);
			ClearJobs(engine);
			ClearTickets(engine);
			ClearOrderManagerData(engine);

			// Clear the generated SRM protocols
			// Comment this in case not needed
			//ClearSrmProtocols(engine);
		}

		private static void ClearSrmData(Engine engine)
		{
			var resourceManager = new ResourceManagerHelper();
			resourceManager.RequestResponseEvent += (s, e) => e.responseMessage = Engine.SLNet.SendSingleResponseMessage(e.requestMessage);

			// remove all reservations
			if (RemoveAllReservations(engine, resourceManager))
			{
				engine.Log("(ClearSrmData) Reservation instances are deleted");
			}
			// Used in case RemoveAllReservations would throw an out of memory exception
			else if (RemoveReservationsInBulk(engine, resourceManager))
			{
				engine.Log("(ClearSrmData) Reservation instances are deleted");
			}
			else
			{
				engine.Log("(ClearSrmData) Unable to remove reservation instances");
			}

			var serviceHelper = new ServiceManagerHelper();
			serviceHelper.RequestResponseEvent += (s, e) => e.responseMessage = Engine.SLNet.SendSingleResponseMessage(e.requestMessage);

			// remove all SRMServiceInfo
			try
			{
				var srmServiceInfo = serviceHelper.GetSRMServiceInfo(SRMServiceInfoExposers.Name.NotEqual(String.Empty)).ToArray();
				if (srmServiceInfo.Any())
				{
					engine.Log("(ClearSrmData) Removing SRM service info: " + srmServiceInfo.Length);
					serviceHelper.RemoveSRMServiceInfo(srmServiceInfo);
					engine.Log("(ClearSrmData) Removing SRM service completed");
				}
			}
			catch (Exception e)
			{
				engine.Log("[ClearSrmData]Error removing SRM Service Info: " + e);
			}

			// remove all SRMSettableServiceStates
			try
			{
				var srmSettableServiceStates = serviceHelper.SRMSettableServiceStates.Read(SRMSettableServiceStateExposers.ServiceId.NotEqual(""));
				if (srmSettableServiceStates.Any())
				{
					engine.Log("(ClearSrmData) Removing SRM settable service state: " + srmSettableServiceStates.Count);
					foreach (var srmSettableServiceState in srmSettableServiceStates) serviceHelper.SRMSettableServiceStates.Delete(srmSettableServiceState);
					engine.Log("(ClearSrmData) Removing SRM settable service state completed");
				}
			
			}
			catch (Exception e)
			{
				engine.Log("[ClearSrmData]Error removing SRM Settable Service States: " + e);
			}

			// remove all generated service definitions
			try
			{
				// all static service definitions start with an '_' in the name
				var serviceDefinitions = serviceHelper.GetServiceDefinitions(ServiceDefinitionExposers.IsTemplate.Equal(true)).Where(s => !s.Name.StartsWith("_")).ToArray();
				if (serviceDefinitions.Any())
				{
					engine.Log("(ClearSrmData) Removing service definitions: " + serviceDefinitions.Length);
					if (!serviceHelper.RemoveServiceDefinitions(out var error, serviceDefinitions))
					{
						engine.Log("[ClearSrmData]Error removing service definitions: " + error);
					}
					engine.Log("(ClearSrmData) Removing service definitions completed");
				}
			}
			catch (Exception e)
			{
				engine.Log("[ClearSrmData]Error removing service definitions: " + e);
			}

			// remove all contributed resources
			try
			{
				// first get the pool ids for the contributing resource pools
				var resourcePools = resourceManager.GetResourcePools();
				var contributingResourcePoolGuids = resourcePools.Where(r => contributingResourcePoolNames.Contains(r.Name)).Select(r => r.GUID);

				var resourcePoolGuidFilter = new ORFilterElement<Resource>(contributingResourcePoolGuids.Select(guid => new ORFilterElement<Resource>(ResourceExposers.PoolGUIDs.Contains(guid))).ToArray());

				// get the resources from the contributing resource pools
				var resources = resourceManager.GetResources(resourcePoolGuidFilter);
				if (resources.Any())
				{
					engine.Log("(ClearSrmData) Removing contributing resources: " + resources.Length);
					resourceManager.RemoveResources(resources);
					engine.Log("(ClearSrmData) Removing contributing resources completed");
				}
			}
			catch (Exception e)
			{
				engine.Log("[ClearSrmData]Error removing resources: " + e);
			}
		}

		private static bool RemoveAllReservations(Engine engine, ResourceManagerHelper resourceManager)
		{
			try
			{
				var reservationInstances = resourceManager.GetReservationInstances(ReservationInstanceExposers.Start.GreaterThanOrEqual(DateTime.MinValue));
				if (reservationInstances.Any())
				{
					engine.Log("(ClearSrmData) Removing reservations: " + reservationInstances.Count());

					resourceManager.RemoveReservationInstances(reservationInstances);

					engine.Log("(ClearSrmData) Removing reservations completed");
					return true;
				}
				else
				{
					engine.Log("(ClearSrmData) Removing reservations: No reservations found");
					return true;
				}
			}
			catch (Exception e)
			{
				engine.Log("[ClearSrmData]Error removing reservations: " + e);
				return false;
			}
		}

		private static bool RemoveReservationsInBulk(Engine engine, ResourceManagerHelper resourceManager)
		{
			try
			{
				var reservationInstances = resourceManager.GetReservationInstances(ReservationInstanceExposers.Start.GreaterThanOrEqual(DateTime.MinValue));
				if (!reservationInstances.Any())
				{
					engine.Log("(ClearSrmData) Removing reservations: No reservations found");
					return true;
				}

				engine.Log($"(ClearSrmData) Clearing {reservationInstances.Count()} reservations");

				for (int start = 0; start < reservationInstances.Length; start += 1000)
				{
					int end = start + 1000;
					if (end >= reservationInstances.Length) end = reservationInstances.Length - 1;

					var reservationsToRemove = new ReservationInstance[end - start];

					Array.Copy(reservationInstances, start, reservationsToRemove, 0, reservationsToRemove.Length);

					engine.Log($"(ClearSrmData) Removing first {end} reservations");

					resourceManager.RemoveReservationInstances(reservationsToRemove);

					engine.Log($"(ClearSrmData) Removing first {end} reservations completed");
				}

				return true;
			}
			catch (Exception e)
			{
				engine.Log("[ClearSrmData]Error removing reservations: " + e);
				return false;
			}
		}

		private static void ClearServices(Engine engine)
		{
			try
			{
				var messages = Engine.SLNet.SendMessage(new GetInfoMessage(InfoType.ServiceInfo));
				engine.Log("(ClearServices) Removing services: " + messages.Length);

				foreach (DMSMessage message in messages)
				{
					ServiceInfoEventMessage service = message as ServiceInfoEventMessage;
					if (service == null) continue;

					// skip templates
					if (service.Type != ServiceType.Service) continue;

					// skip services that shouldn't be removed
					if (serviceNamesToKeep.Contains(service.Name)) continue;

					engine.SendSLNetMessage(new SetDataMinerInfoMessage
					{
						Uia1 = new UIA(new[] { (uint)service.DataMinerID, (uint)service.ElementID }),
						What = 74
					});
				}

				engine.Log("(ClearServices) Removing services completed");
			}
			catch (Exception e)
			{
				engine.Log("[ClearServices]Error removing services: " + e);
			}
		}

		private static void ClearJobs(Engine engine)
		{
			var jobManagerHelper = new JobManagerHelper(m => Engine.SLNet.SendMessages(m));

			try
			{
				var jobs = jobManagerHelper.Jobs.Read(JobExposers.FieldValues.JobStartGreaterThan(DateTime.MinValue));
				engine.Log("(ClearJobs) Removing jobs: " + jobs.Count);

				foreach (var job in jobs)
				{
					jobManagerHelper.Jobs.Delete(job);
				}

				engine.Log("(ClearJobs) Removing jobs completed");
			}
			catch (Exception e)
			{
				engine.Log("[ClearJobs]Error removing jobs: " + e);
			}
		}

		private void ClearTickets(Engine engine)
		{
			var ticketingHelper = new TicketingGatewayHelper { HandleEventsAsync = false };
			ticketingHelper.RequestResponseEvent += (s, e) => e.responseMessage = Engine.SLNet.SendSingleResponseMessage(e.requestMessage);

			// remove all user tasks
			try
			{
				var userTasksTicketFieldResolver = ticketingHelper.GetTicketFieldResolvers(TicketFieldResolver.Factory.CreateEmptyResolver(userTasksTicketDomainName)).FirstOrDefault() ?? throw new InvalidOperationException("Could not find user task ticket field resolver");
				var userTaskTickets = ticketingHelper.GetTickets(null, TicketingExposers.ResolverID.Equal(userTasksTicketFieldResolver.ID));
				List<Ticket> ticketsToRemove = new List<Ticket>();
				foreach (Ticket ticket in userTaskTickets)
				{
					if (ticket.CustomTicketFields.TryGetValue(IngestExportFkTicketField, out object nonLiveForeignKey) && !String.IsNullOrEmpty(Convert.ToString(nonLiveForeignKey)) && !clearNonLive)
					{
						engine.Log($"ClearTickets|User Tasks|Skipped removing ticket {ticket.ID.ToString()} because it contains an Ingest Export FK {nonLiveForeignKey}");
						continue; // Non-Live user task
					}

					if (!ticket.CustomTicketFields.TryGetValue(ServiceIdTicketField, out object serviceId) || String.IsNullOrEmpty(Convert.ToString(serviceId)) && !clearNonLive)
					{
						engine.Log($"ClearTickets|User Tasks|Skipped removing ticket {ticket.ID.ToString()} because it has an empty Service ID field");
						continue; // Possible Non-Live user task
					}

					ticketsToRemove.Add(ticket);
				}

				engine.Log($"ClearTickets|User Tasks|User task tickets to remove {ticketsToRemove.Count} | {String.Join(", ", ticketsToRemove.Select(x => x.ID.ToString()))}");
				//RemoveTickets(engine, userTasksTicketDomainName, ticketingHelper, ticketsToRemove);
			}
			catch (Exception e)
			{
				engine.Log("[ClearTickets]Error removing user task tickets: " + e);
			}

			// remove all ingest/export tickets
			//if (ClearNonLive)
			//{
			//	try
			//	{
			//		var ingestExportTicketFieldResolver = ticketingHelper.GetTicketFieldResolvers(TicketFieldResolver.Factory.CreateEmptyResolver(ingestExportTicketDomainName)).FirstOrDefault();
			//		var ingestExportTickets = ticketingHelper.GetTickets(null, TicketingExposers.ResolverID.Equal(ingestExportTicketFieldResolver.ID));
			//		RemoveTickets(engine, ingestExportTicketDomainName, ticketingHelper, ingestExportTickets);
			//	}
			//	catch (Exception e)
			//	{
			//		engine.Log("[ClearTickets]Error removing ingest/export tickets: " + e);
			//	}
			//}

			// remove all notes tickets
			try
			{
				var notesTicketFieldResolver = ticketingHelper.GetTicketFieldResolvers(TicketFieldResolver.Factory.CreateEmptyResolver(notesTicketDomainName)).FirstOrDefault() ?? throw new InvalidOperationException("Could not find ticket field resolver");
				var notesTickets = ticketingHelper.GetTickets(null, TicketingExposers.ResolverID.Equal(notesTicketFieldResolver.ID));
				RemoveTickets(engine, notesTicketDomainName, ticketingHelper, notesTickets);
			}
			catch (Exception e)
			{
				engine.Log("[ClearTickets]Error removing notes tickets: " + e);
			}
		}

		private static void RemoveTickets(IEngine engine, string domain, TicketingGatewayHelper ticketingHelper, IEnumerable<Ticket> ticketsToRemove)
		{
			if (ticketsToRemove.Any())
			{
				engine.Log($"{nameof(RemoveTickets)}|{domain}|Removing tickets: " + ticketsToRemove.Count());

				if (!ticketingHelper.RemoveTickets(out var error, ticketsToRemove.ToArray()))
				{
					engine.Log($"{nameof(RemoveTickets)}|{domain}|Error removing user task tickets: " + error);
				}

				engine.Log($"{nameof(RemoveTickets)}|{domain}|Removing tickets completed");
			}
		}

		private static void ClearOrderManagerData(Engine engine)
		{
			var orderManagerElement = engine.FindElementsByProtocol("Finnish Broadcasting Company Order Manager").FirstOrDefault();
			if (orderManagerElement == null)
			{
				engine.Log("(ClearOrderManagerData) Order Manager element not found");
				return;
			}

			if (!orderManagerElement.IsActive)
			{
				engine.Log("(ClearOrderManagerData) Order Manager element not active");
				return;
			}

			try
			{
				var orderReservationIds = orderManagerElement.GetTablePrimaryKeys(2000);
				foreach (var orderReservationId in orderReservationIds) orderManagerElement.SetParameterByPrimaryKey(2006, orderReservationId, "Delete");

				var eventJobIds = orderManagerElement.GetTablePrimaryKeys(2500);
				foreach (var eventJobId in eventJobIds) orderManagerElement.SetParameterByPrimaryKey(2507, eventJobId, "Delete");
			}
			catch (Exception e)
			{
				engine.Log("(ClearOrderManagerData) Exception clearing data from order manager: " + e);
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "<Pending>")]
		private static void ClearSrmProtocols(Engine engine)
		{
			try
			{
				engine.Log("(ClearSrmProtocols) Removing SRM protocols");

				var functions = (GetProtocolFunctionsResponseMessage)Engine.SLNet.SendSingleResponseMessage(new GetProtocolFunctionsMessage());

				var messages = engine.SendSLNetMessage(new GetInfoMessage(InfoType.Protocols));
				foreach (DMSMessage message in messages)
				{
					GetProtocolsResponseMessage protocolInfoMessage = message as GetProtocolsResponseMessage;
					if (protocolInfoMessage == null) continue;

					if (!protocolInfoMessage.Protocol.StartsWith("_")) continue;

					var protocolFunctions = functions.Functions.Where(f => f.ProtocolName == protocolInfoMessage.Protocol);
					DeleteFunctionsXml(engine, protocolInfoMessage.Protocol, protocolFunctions);

					DeleteProtocolVersions(engine, protocolInfoMessage.Protocol, protocolInfoMessage.Versions);
				}

				engine.Log("(ClearSrmProtocols) Removing ingest/export tickets completed");
			}
			catch (Exception e)
			{
				engine.Log("[ClearSrmProtocols]Error removing SRM protocols: " + e);
			}
		}	

		private static void DeleteFunctionsXml(Engine engine, string protocol, IEnumerable<ProtocolFunction> protocolFunctions)
		{
			foreach (var protocolFunction in protocolFunctions)
			{
				foreach (var protocolFunctionVersion in protocolFunction.ProtocolFunctionVersions)
				{
					try
					{
						Engine.SLNet.SendSingleResponseMessage(new DeleteFunctionsXmlRequestMessage
						{
							FileName = protocolFunctionVersion.FileName,
							Force = true,
							ProtocolName = protocol
						});
					}
					catch (Exception)
					{
						engine.Log("[ClearSrmProtocols]Error removing SRM protocol function: " + protocol);
					}
				}
			}
		}

		private static void DeleteProtocolVersions(Engine engine, string protocol, IEnumerable<string> versions)
		{
			foreach (string version in versions)
			{
				try
				{
					Engine.SLNet.SendSingleResponseMessage(new DeleteProtocolFileMessage
					{
						Force = true,
						ProtocolName = protocol,
						ProtocolVersion = "",
						Sa = new SA(new[] { protocol }),
						What = 0
					});
				}
				catch(Exception e)
				{
					engine.Log($"[ClearSrmProtocols]Error removing SRM protocol version: {protocol}.{version}:  {e}");
				}
			}
		}
	}
}