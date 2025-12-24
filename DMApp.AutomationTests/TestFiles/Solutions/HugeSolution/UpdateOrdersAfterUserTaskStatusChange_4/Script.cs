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

namespace UpdateOrdersAfterUserTaskStatusChange_4
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private Helpers helpers;
		private Service service;
		private bool disposedValue;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			try
			{
				Initialize(engine);

				if (!GetService()) return;

				foreach (var orderReference in service.OrderReferences)
				{
					var order = helpers.OrderManager.GetOrder(orderReference);

					var hmxRoutingAndEndpointServices = order.AllServices.Where(s => s.IsHmxRouting || s.Definition.IsEndPointService).ToList();

					foreach (var hmxRoutingOrEndpointService in hmxRoutingAndEndpointServices)
					{
						hmxRoutingOrEndpointService.TryUpdateMcrStatus(helpers, order);
					}

					order.UpdateUiProperties(helpers);
				}
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Something went wrong during update order after user task update: {e}");
			}
			finally
			{
				Dispose();
			}
		}

		private void Initialize(Engine engine)
		{
			engine.SetFlag(RunTimeFlags.NoKeyCaching);

			this.helpers = new Helpers(engine, Scripts.UpdateOrdersAfterUserTaskStatusChange);
		}

		private bool GetService()
		{
			string serviceId = helpers.Engine.GetScriptParam(1).Value;
			bool gettingServiceIdFailed = !Guid.TryParse(serviceId, out var serviceGuid);
			if (gettingServiceIdFailed)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Unable to convert script param '{serviceId}' to a valid GUID");
				return false;
			}

			bool gettingServiceFailed = !helpers.ServiceManager.TryGetService(serviceGuid, out this.service);
			if (gettingServiceFailed)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Unable to get service {serviceGuid}");
				return false;
			}

			helpers.AddOrderReferencesForLogging(service.OrderReferences.ToArray());

			return true;
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