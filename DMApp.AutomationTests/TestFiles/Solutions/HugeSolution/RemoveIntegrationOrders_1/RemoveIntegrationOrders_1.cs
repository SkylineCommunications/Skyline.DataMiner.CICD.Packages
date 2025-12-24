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
using System.Linq;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Core.DataMinerSystem.Common;
using Skyline.DataMiner.Library.Solutions.SRM;
using Skyline.DataMiner.Net.ResourceManager.Objects;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script
{
	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
	{
		var dms = Engine.SLNetRaw.GetDms();
		var orderManager = dms.GetElement("Order Manager");

		engine.Timeout = TimeSpan.FromHours(12);

		var integration = engine.GetScriptParam("integration").Value;

		var integrationOrders = SrmManagers.ResourceManager.GetReservationInstancesByProperty("Integration", integration);
		foreach (var integrationOrder in integrationOrders)
		{
			try
			{
				var virtualPlatform = integrationOrder.Properties.FirstOrDefault(p => p.Key == "Virtual Platform").Value;
				if (Convert.ToString(virtualPlatform) != "Order") continue;

				ForceDeleteOrder(engine, integrationOrder);
				RemoveReferenceFromOrderManager(orderManager, integrationOrder);
			}
			catch (Exception e)
			{
				engine.Log($"(ForceDeleteOrder) Removing order {integrationOrder.ID} failed: {e}");
			}
		}
	}

	private void ForceDeleteOrder(Engine engine, ReservationInstance order)
	{
		var forceDeleteOrderScript = engine.PrepareSubScript("ForceDeleteOrder");
		forceDeleteOrderScript.Synchronous = true;
		forceDeleteOrderScript.PerformChecks = false;

		forceDeleteOrderScript.SelectScriptParam("Order Name", order.Name);

		forceDeleteOrderScript.StartScript();
	}

	private void RemoveReferenceFromOrderManager(IDmsElement orderManager, ReservationInstance order)
	{
		var integrationsTable = orderManager.GetTable(1200);

		var columnFilter = new ColumnFilter
		{
			Pid = 1208,
			Value = order.ID.ToString(),
			ComparisonOperator = ComparisonOperator.Equal
		};

		var row = integrationsTable.QueryData(new[] { columnFilter }).FirstOrDefault();
		if (row == null) return;

		var key = Convert.ToString(row[0]);
		integrationsTable.DeleteRow(key);
	}
}