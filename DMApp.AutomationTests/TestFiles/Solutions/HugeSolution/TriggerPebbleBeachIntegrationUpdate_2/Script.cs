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

27/01/2020	1.0.0.1		JVT, Skyline	Initial Version
****************************************************************************
*/

namespace TriggerPebbleBeachIntegrationUpdate_2
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private const int PebbleBeachUpdatedEventIdsParameterId = 60;
		private const int PebbleBeachEventsTableParameterId = 2100;
		private const int PebbleBeachEventsTablePlasmaIdIdx = 4;
		private const int PebbleBeachEventsTableEventIdIdx = 0;

		private Helpers helpers;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			helpers = new Helpers(engine, Scripts.TriggerPebbleBeachIntegrationUpdate);

			engine.SetFlag(RunTimeFlags.NoKeyCaching);
			engine.Timeout = TimeSpan.FromHours(10);

			string orderId = Convert.ToString(engine.GetScriptParam(1).Value);

			Guid orderGuid;
			if (!Guid.TryParse(orderId, out orderGuid))
			{
				engine.ExitFail("Unable to parse Guid");
				return;
			}

			LiteOrder order = new OrderManager(helpers).GetLiteOrder(orderGuid);
			if (order == null)
			{
				engine.ExitFail("Unable to get Order");
				return;
			}

			if (String.IsNullOrWhiteSpace(order.PlasmaId)) return;

			OrderManagerElement orderManagerElement = new OrderManagerElement(helpers);

			IDmsElement pebbleBeachElement = orderManagerElement.PebbleBeachElement;

			if (pebbleBeachElement == null)
			{
				engine.ExitFail("Unable to find Pebble Beach Element");
				return;
			}

			IDmsStandaloneParameter<string> updatedEventIdsParameter = pebbleBeachElement.GetStandaloneParameter<string>(PebbleBeachUpdatedEventIdsParameterId);

			object[][] pebbleBeachEventsTable = pebbleBeachElement.GetTable(PebbleBeachEventsTableParameterId).GetRows();
			foreach (object[] row in pebbleBeachEventsTable)
			{
				if (Convert.ToString(row[PebbleBeachEventsTablePlasmaIdIdx]) == order.PlasmaId)
				{
					updatedEventIdsParameter.SetValue(Convert.ToString(row[PebbleBeachEventsTableEventIdIdx]));
					return;
				}
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

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

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		~Script()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(false);
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}