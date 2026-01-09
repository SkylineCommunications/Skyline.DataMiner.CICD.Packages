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

namespace HandleServiceStartFailure_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Automation;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.StartBookingFailure;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private Helpers helpers;
		private bool disposedValue;

		/// <summary>
		/// Script entry point.
		/// </summary>
		/// <param name="engine">Link to DataMiner.</param>
		public void Run(Engine engine)
		{
			helpers = new Helpers(engine, Scripts.HandleServiceStartFailure);

			try
			{
				// Attention: This line needs to be always like this, if the custom converter is not used, it will throw an exception
				var errorData = engine.GetScriptParamValue(
					"ErrorData",
					rawData => JsonConvert.DeserializeObject<List<StartActionsFailureErrorData>>(
						rawData,
						new StartActionsFailureErrorDataConverter()
					)
				);

				// Will handle the errors.
				// Custom handling can be implemented in HandlerError
				HandleError(errorData);

				Dispose();
			}
			catch (Exception e)
			{
				string scriptInput = engine.GetScriptParam("ErrorData").Value;

				NotificationManager.SendMailToSkylineDevelopers(helpers, "Service Start Failure", $"An error occurred while executing the HandleServiceStartFailure script with input: {scriptInput}");

				engine.GenerateInformation($"Script Booking Start Failure Template failed due to: {e}");

				Dispose();

				// Optional: Reports the correct exception to the caller script
				AutomationScript.HandleException(engine, e);
			}
		}

		public void HandleError(List<StartActionsFailureErrorData> errorData)
		{
			foreach (var item in errorData)
			{
				string errorMessage = GetErrorMessage(item);

				var serviceId = item.ReservationInstanceId;
				var service = serviceId.HasValue ? helpers.ServiceManager.GetService(serviceId.Value) : null;

				if (service == null)
				{
					helpers.Log(nameof(Script), nameof(HandleError), $"Unknown service could not be started: {errorMessage}");
					continue; 
				}

				if (service.OrderReferences == null || !service.OrderReferences.Any())
				{
					helpers.Log(nameof(Script), nameof(HandleError), $"Service {item.ReservationInstanceId} could not be started: {errorMessage}");
					continue;
				}

				string message = $"Service {service.Name} ({service.Id}) part of Order(s) {string.Join(", ", service.OrderReferences)} could not be started:\n {errorMessage}";

				NotificationManager.SendMailToSkylineDevelopers(helpers, $"Service Start Failure", message);

				helpers.Log(nameof(Script), nameof(HandleError), message);

				helpers.AddOrderReferencesForLogging(service.OrderReferences.ToArray());
			}
		}

		private static string GetErrorMessage(StartActionsFailureErrorData item)
		{
			switch (item.ErrorReason)
			{
				case StartActionsFailureErrorData.Reason.UnexpectedException:
					return "Reservation failed to start due an unexpected exception";
				case StartActionsFailureErrorData.Reason.ResourceElementNotActive:
					return $"Reservation failed to start due to resource element {item.Resource.DmaID}/{item.Resource.ElementID} was not set to active when the reservation started";
				case StartActionsFailureErrorData.Reason.ReservationInstanceNotFound:
					return "Reservation failed to start due to reservation instance not found.";
				default:
					return "Reservation failed to start due an unknown reason.";
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

		~Script()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}