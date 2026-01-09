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

namespace RemoveOldServices_1
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Library.Automation;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using System.Linq;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script
	{
		private const string OrderManagerElementName = "Order Manager";
		private const int orderServiceDeletionDelayPid = 2200;
		private const string HandleOrderActionScriptName = "HandleOrderAction";

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			engine.SetFlag(RunTimeFlags.NoKeyCaching);
			engine.Timeout = TimeSpan.FromHours(10);

			// Get Order Manager element
			var dms = Engine.SLNetRaw.GetDms();
			var orderManager = dms.GetElement(OrderManagerElementName);

			if (orderManager == null)
			{
				engine.Log(nameof(Script), nameof(Run), $"Unable to find {OrderManagerElementName} element");
				return;
			}

			// Get service deletion delay
			int? delayInMinutes = orderManager.GetStandaloneParameter<int?>(orderServiceDeletionDelayPid).GetValue();
			if (delayInMinutes == null)
			{
				engine.Log(nameof(Script), nameof(Run), $"Unable to retrieve service deletion delay parameter {orderServiceDeletionDelayPid}");
				return;
			}

			DateTime nowMinusDelay = DateTime.Now.Subtract(TimeSpan.FromMinutes((int)delayInMinutes));

			FilterElement<ReservationInstance> filter = new ANDFilterElement<ReservationInstance>(ReservationInstanceExposers.End.LessThan(nowMinusDelay))
				.AND(ReservationInstanceExposers.Properties.DictStringField("Booking Manager").Equal("Order Booking Manager"));

			var oldOrders = SrmManagers.ResourceManager.GetReservationInstances(filter);

			for (int i = 0; i < oldOrders.Length; i++)
			{
				var oldOrder = oldOrders[i];

				engine.Log(nameof(Script), nameof(Run), $"[{(i + 1)}/{oldOrders.Length}] Removing services for Order {oldOrder.Name}...");

				try
				{
					var handleOrderActionInfo = new HandleOrderActionInfo
					{
						Action = "Delete Services",
						OrderId = oldOrder.ID.ToString(),
						RemoveAllServices = true,
						ServiceIds = new List<string>()
					};

					engine.SendSLNetSingleResponseMessage(new ExecuteScriptMessage(HandleOrderActionScriptName)
					{
						Options = new SA(new[]
						{
							$"PARAMETER:3:{handleOrderActionInfo.Serialize()}",
							$"PARAMETER:4:{-1}",
							$"PARAMETER:5:{oldOrder.ID}",
							"OPTIONS:0",
							"CHECKSETS:FALSE",
							"EXTENDED_ERROR_INFO",
							"DEFER:FALSE" // synchronous execution
						})
					});

					engine.Log(nameof(Script), nameof(Run), $"Removing services for Order {oldOrder.Name} was successful");
				}
				catch (Exception e)
				{
					engine.Log(nameof(Script), nameof(Run), $"Removing services for Order {oldOrder.Name} failed {e}");
				}
			}
		}
	}
}