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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using Skyline.DataMiner.Net.Messages.SLDataGateway;
using Skyline.DataMiner.Net.ResourceManager.Objects;
using Skyline.DataMiner.Utils.YLE.Integrations;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script
{
	private Helpers helpers;

	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
	{
		helpers = new Helpers(engine, Scripts.ReprocessOrders);

		helpers.LogMethodStart(nameof(Script), nameof(Run), out var stopwatch);

		try
		{
			int slidingWindowSizeInDays = Convert.ToInt32(engine.GetScriptParam(2).Value);
			int maxAmountOfRetries = Convert.ToInt32(engine.GetScriptParam(3).Value);

			var now = DateTime.Now;

			var videoTypeFilter = ReservationInstanceExposers.Properties.DictStringField(LiteOrder.PropertyNameType).Equal(OrderType.Video.GetDescription());
			var integrationTypeFilter = ReservationInstanceExposers.Properties.DictStringField(LiteOrder.PropertyNameIntegration).NotEqual(IntegrationType.None.GetDescription());
			var lowerStartTimeFilter = ReservationInstanceExposers.Start.GreaterThan(now);
			var upperStartTimeFilter = ReservationInstanceExposers.Start.LessThan(now.AddDays(slidingWindowSizeInDays));

			var combinedFilter = videoTypeFilter.AND(integrationTypeFilter).AND(lowerStartTimeFilter).AND(upperStartTimeFilter);

			var orderReservations = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, combinedFilter);

			var ordersToReprocess = new HashSet<OrderToReprocess>();
			foreach (var orderReservation in orderReservations)
			{
				int currentRetryCounterValue = helpers.OrderManagerElement.GetResourceOverbookedRetryCounter(orderReservation.ID);
				if (currentRetryCounterValue >= maxAmountOfRetries) continue;

				var orderToReprocess = new OrderToReprocess { OrderReservation = orderReservation, CurrentRetryCounterValue = currentRetryCounterValue };

				foreach (var contributingResource in orderReservation.ResourcesInReservationInstance)
				{
					try
					{
						var serviceReservation = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, contributingResource.GUID) ?? throw new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.ReservationNotFoundException(contributingResource.GUID);

						string serviceStatus = Convert.ToString(serviceReservation.Properties.Dictionary[ServicePropertyNames.Status]);

						helpers.Log(nameof(Script), nameof(Run), $"Service {serviceReservation.Name} from order {orderReservation.Name} has status {serviceStatus}");

						if (serviceStatus == Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Status.ResourceOverbooked.GetDescription())
						{
							orderToReprocess.ServiceReservations.Add(serviceReservation);
						}
					}
					catch(Exception ex)
					{
						helpers.Log(nameof(Script), nameof(Run), $"Exception while checking order {orderReservation.Name}({orderReservation.ID}) contributing resource {contributingResource.GUID}: {ex}");
					}
				}

				if (orderToReprocess.ServiceReservations.Any())
				{
					ordersToReprocess.Add(orderToReprocess);
				}
			}

			string emailTitle = $"Reprocess Resource Overbooked Integration Orders";
			string emailContent = $"Sliding window size in days: {slidingWindowSizeInDays}<br>Max amount of retries: {maxAmountOfRetries}<br>Reprocessed orders: <br>{string.Join("<br>", ordersToReprocess.Select(o => $"Retry {o.CurrentRetryCounterValue + 1} for Order {o.OrderReservation.Name} ({o.OrderReservation.ID}) from {o.OrderReservation.Start} until {o.OrderReservation.End} because of services {string.Join(", ", o.ServiceReservations.Select(s => s.Name))}"))}";

			helpers.Log(nameof(Script), nameof(Run), emailContent.Replace("<br>", "\n"));

			if (ordersToReprocess.Any()) NotificationManager.SendMailToSkylineDevelopers(helpers, emailTitle, emailContent);
			NotificationManager.SendMailToTeemuJussiJariJuho(helpers, emailTitle, emailContent);

			var orderIdsToReprocess = ordersToReprocess.Select(o => o.OrderReservation.ID).ToArray();

			helpers.OrderManagerElement.ReprocessIntegrationOrders(orderIdsToReprocess);

			helpers.OrderManagerElement.IncreaseResourceOverbookedRetryCounter(orderIdsToReprocess);
		}
		catch (Exception ex)
		{
			helpers.Log(nameof(Script), nameof(Run), $"Exception while executing script: {ex}");
		}
		finally
		{
			helpers.LogMethodCompleted(nameof(Script), nameof(Run), null, stopwatch);
			helpers.Dispose();
		}
	}

	private class OrderToReprocess
	{
		public ReservationInstance OrderReservation { get; set; }

		public List<ReservationInstance> ServiceReservations { get; set; } = new List<ReservationInstance>();

		public int CurrentRetryCounterValue { get; set; }
	}
}