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
using Skyline.DataMiner.Library.Solutions.SRM;
using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Net.Messages.SLDataGateway;
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
		var resourceName = engine.GetScriptParam("Resource Name").Value;

		var resource = SrmManagers.ResourceManager.GetResources(ResourceExposers.Name.Equal(resourceName)).FirstOrDefault();
		if (resource == null)
		{
			engine.Log($"(ForceDeleteResource) Resource {resourceName} not found");
			return;
		}

		var force = Convert.ToBoolean(engine.GetScriptParam("Force").Value);
		if (force) RemoveReservationsUsingResource(resource);

		var setResourceMessage = new SetResourceMessage(resource)
		{
			isDelete = true,
			ForceQuarantine = force,
			IgnorePastReservations = true,
			//IgnoreCanceledReservations = true
		};

		engine.SendSLNetSingleResponseMessage(setResourceMessage);
	}

	private static void RemoveReservationsUsingResource(Resource resource)
	{
		FilterElement<ReservationInstance> filter = ReservationInstanceExposers.Status.NotEqual((int)ReservationStatus.Canceled);
		filter = filter.AND(ReservationInstanceExposers.End.GreaterThanOrEqual(DateTime.UtcNow));
		filter = filter.AND(ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(resource.ID));

		var reservationInstances = SrmManagers.ResourceManager.GetReservationInstances(filter);
		if (!reservationInstances.Any()) return;

		SrmManagers.ResourceManager.RemoveReservationInstances(reservationInstances);
	}
}