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

namespace HandleOrderStartFailure_1
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	class Script
	{
		[AutomationEntryPoint(AutomationEntryPointType.Types.OnSrmStartActionsFailure)]
		public void OnSrmStartActionsFailure(Engine engine, List<StartActionsFailureErrorData> errorData)
		{
			foreach (var item in errorData)
			{
				string errorMessage = "";
				switch (item.ErrorReason)
				{
					case StartActionsFailureErrorData.Reason.UnexpectedException:
						errorMessage = "Reservation failed to start due an unexpected exception";
						break;
					case StartActionsFailureErrorData.Reason.ResourceElementNotActive:
						errorMessage = $"Reservation failed to start due to resource element {item.Resource.DmaID}/{item.Resource.ElementID} was not set to active when the reservation started";
						break;
					case StartActionsFailureErrorData.Reason.ReservationInstanceNotFound:
						errorMessage = "Reservation failed to start due to reservation instance not found.";
						break;
					default:
						errorMessage = "Reservation failed to start due an unknown reason.";
						break;
				}

				var orderId = item.ReservationInstanceId;
				if (!orderId.HasValue)
				{
					engine.Log($"HandleOrderStartFailure|OnSrmStartActionsFailure|Unknown order could not be started: {errorMessage}");
				}
				else
				{
					LogErrorInOrderLogging(engine, orderId.Value, errorMessage);
				}
			}
		}

		private void LogErrorInOrderLogging(Engine engine, Guid orderId, string errorMessage)
		{
			try
			{
				var helpers = new Helpers(engine, Scripts.HandleOrderStartFailure, orderIds:orderId);
				helpers.Log("HandleOrderStartFailure", "LogErrorInOrderLogging", $"Order {orderId} could not be started: {errorMessage}");
			}
			catch (Exception)
			{
				engine.Log($"HandleOrderStartFailure|OnSrmStartActionsFailure|Order {orderId} could not be started: {errorMessage}");
			}
		}
	}
}