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

namespace HandleOrderAction_1
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages;

	public class Script : IDisposable
	{
		private Helpers helpers;

		private bool triggeredByBookingAction;
		private string inputAction;
		private Guid inputOrderId;
		private List<string> inputServiceIdsToRemove;
		private bool removeAllServices;

		public void Run(IEngine engine)
		{
			bool lockRetrieved = false;
			OrderAction action = null;
			
			try
			{
				Initialize(engine);

				if (triggeredByBookingAction)
				{
					StartUpdateOrderStatusAsyncScript();
				}
				else
				{
					action = GetAction();
					if (!(action is DeleteServicesAction))
					{
						RetrieveLock(inputOrderId);
						lockRetrieved = true;
					}

					action.Execute();
				}
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Exception handling order action: {e}");

				action?.HandleException(e.Message);
			}
			finally
			{
				Dispose();
				if (lockRetrieved) ReleaseLock();
			}
		}

		private void Initialize(IEngine engine)
		{
			engine.Timeout = TimeSpan.FromHours(1);
			engine.SetFlag(RunTimeFlags.NoKeyCaching);
			helpers = new Helpers(engine, Scripts.HandleOrderAction);

			var action = engine.GetScriptParam("Action").Value;

			inputOrderId = Guid.Parse(engine.GetScriptParam("ReservationGuid").Value);
			helpers.AddOrderReferencesForLogging(inputOrderId);

			inputAction = action;
			var handleOrderActionInfo = HandleOrderActionInfo.Deserialize(action); // Will succeed when Order Manager Element triggers this script.

			if (handleOrderActionInfo == null)
			{
				triggeredByBookingAction = true;
				return;
			}

			inputAction = handleOrderActionInfo.Action;
			inputServiceIdsToRemove = handleOrderActionInfo.ServiceIds ?? new List<string>();
			removeAllServices = handleOrderActionInfo.RemoveAllServices;
		}

		private OrderAction GetAction()
		{									
			switch (inputAction)
			{
				case "Book Services":
					return new BookServicesAction(helpers, inputOrderId);
				case "Book Event Level Reception Services":
					return new BookEventLevelReceptionServicesAction(helpers, inputOrderId);
				case "Delete Services":
					return new DeleteServicesAction(helpers, inputOrderId, inputServiceIdsToRemove, removeAllServices);
				default:
					throw new InvalidOperationException("Invalid action: " + inputAction);
			}
		}

		private void StartUpdateOrderStatusAsyncScript()
		{
			helpers.Log(nameof(Script), nameof(StartUpdateOrderStatusAsyncScript), $"Launching script UpdateOrderStatus asynchronous from Handle Order Action");

			helpers.Engine.SendSLNetSingleResponseMessage(new ExecuteScriptMessage("UpdateOrderStatus")
			{
				Options = new SA(new[]
				{
					$"PARAMETER:3:{inputAction}",
					$"PARAMETER:4:{inputOrderId}",
					"OPTIONS:0",
					"CHECKSETS:FALSE",
					"EXTENDED_ERROR_INFO",
					"DEFER:TRUE" // async execution
				})
			});
		}

		private void RetrieveLock(Guid reservationId)
		{
			try
			{
				var lockInfo = helpers.LockManager.RequestOrderLock(reservationId);
				if (!lockInfo.IsLockGranted)
				{
					helpers.Log(nameof(Script), "RetrieveLock", "Lock could not be retrieved");
					throw new LockNotGrantedException();
				}
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(RetrieveLock), $"Exception retrieving order lock: {e}");
				throw;
			}
		}

		private void ReleaseLock()
		{
			try
			{
				if (helpers.LockManager == null) return;

				helpers.LockManager.ReleaseLocks();
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(ReleaseLock), $"Exception releasing order lock: {e}");
			}
		}

		#region IDisposable Support
		private bool disposedValue;

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
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(false);
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);

			GC.SuppressFinalize(this);
		}
		#endregion
	}
}