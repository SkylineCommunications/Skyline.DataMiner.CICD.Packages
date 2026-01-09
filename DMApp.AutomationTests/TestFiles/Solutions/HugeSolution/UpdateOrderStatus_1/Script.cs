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

namespace UpdateOrdrStatus
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM.LifecycleServiceOrchestration;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.Events;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private Helpers helpers;

		public void Run(Engine engine)
		{
			Initialize(engine);

			try
			{
				var orderId = Guid.Parse(engine.GetScriptParam("Order Id").Value);

				helpers.AddOrderReferencesForLogging(orderId);

				if (!TryGetOrder(orderId, out var order, out var errorMessage))
				{
					Log(nameof(Run), errorMessage);
					Dispose();
					return;
				}
				
				var action = engine.GetScriptParam("Action").Value;

				var enhancedAction = new LsoEnhancedAction(action);

				if (!TryUpdateOrderStatus(order, enhancedAction))
				{
					Log(nameof(Run), errorMessage);
				}
				else
				{
					Log(nameof(Run), $"Updating status for order {order.Name} succeeded");
				}
			}
			catch (Exception e)
			{
				Log(nameof(Run), $"Exception handling update order status: {e}");
			}
			finally
			{
				Dispose();
			}
		}

		private void Initialize(Engine engine)
		{
			engine.Timeout = TimeSpan.FromHours(1);
			engine.SetFlag(RunTimeFlags.NoKeyCaching);

			this.helpers = new Helpers(engine, Scripts.UpdateOrderStatus);
		}

		private bool TryGetOrder(Guid reservationId, out Order order, out string errorMessage)
		{
			order = null;
			errorMessage = string.Empty;

			try
			{
				order = helpers.OrderManager.GetOrder(reservationId);

				errorMessage = order != null ? string.Empty : $"Order with ID {reservationId} could not be found";

				return order != null;
			}
			catch (Exception e)
			{
				errorMessage = $"Exception while retrieving order with ID {reservationId}: {e}";
				return false;
			}
		}

		private bool TryUpdateOrderStatus(Order order, LsoEnhancedAction incomingScriptAction)
		{
			LogMethodStart(nameof(TryUpdateOrderStatus), out var stopWatch);

			if (!DoesOrderStatusNeedsAnUpdate(order, incomingScriptAction))
			{
				LogMethodCompleted(nameof(TryUpdateOrderStatus), stopWatch);
				return true;
			}
				
			// We need to wait until the order reservation has the correct booking life cycle to proceed.
			if (!OrderManager.TryWaitingUntilOrderHasValidBookingLifeCycle(helpers, order, incomingScriptAction, out var orderReservationInstance))
			{
				Log(nameof(TryUpdateOrderStatus), $"Waiting on the correct order booking life cycle didn't succeed");
				LogMethodCompleted(nameof(TryUpdateOrderStatus), stopWatch);
				return false; 
			}

			order.Reservation = orderReservationInstance; // Refresh reservation with new retrieved data from wait logic.
			
			try
			{
				var orderResourcesInReservation = order.Reservation.ResourcesInReservationInstance.Select(r => r as ServiceResourceUsageDefinition).Where(r => r != null).ToList();

				bool orderHasNoLinkedContributings = !orderResourcesInReservation.Any() || orderResourcesInReservation.All(r => r.GUID == Guid.Empty);
				if (orderHasNoLinkedContributings && order.End <= DateTime.Now)
				{
					helpers.Log(nameof(Script), nameof(TryUpdateOrderStatus), $"{nameof(orderHasNoLinkedContributings)}: {orderHasNoLinkedContributings} and order has ended so status will be set to completed");

					// A previous running order which is stopped by the adding service(s) to a running order feature will detach his running services.
					// So order can be directly set to completed as there is no underlying behaviour anymore.
					order.UpdateStatus(helpers, Status.Completed);
				}
				else
				{
					order.RefreshStatusAfterServiceStatusUpdate(helpers);
				}

				order.UpdateUiProperties(helpers);

				Log(nameof(TryUpdateOrderStatus), "Order status successfully updated");
				LogMethodCompleted(nameof(TryUpdateOrderStatus), stopWatch);

				return true;
			}
			catch (Exception e)
			{
				Log(nameof(TryUpdateOrderStatus), $"Exception while updating order status: {e}");
				LogMethodCompleted(nameof(TryUpdateOrderStatus), stopWatch);
				return false;
			}
		}

		private bool DoesOrderStatusNeedsAnUpdate(Order order, LsoEnhancedAction incomingScriptAction)
		{
			LogMethodStart(nameof(DoesOrderStatusNeedsAnUpdate), out var stopWatch);

			bool preRollEndWillUpdateOrderStatus = incomingScriptAction.Event == SrmEvent.START_BOOKING_WITH_PREROLL && order.PreRoll != TimeSpan.Zero;
			bool isTriggeredByPostRollStartAndIsSetToZero = incomingScriptAction.Event == SrmEvent.STOP && order.PostRoll == TimeSpan.Zero;
			bool orderIsAlreadyCancelled = order.Status == Status.Cancelled;
			bool isSavedOrder = order.IsSaved;

			if (preRollEndWillUpdateOrderStatus || isTriggeredByPostRollStartAndIsSetToZero || orderIsAlreadyCancelled || isSavedOrder)
			{
				Log(nameof(TryUpdateOrderStatus), $"No need to update the order status | {nameof(preRollEndWillUpdateOrderStatus)} = {preRollEndWillUpdateOrderStatus}, {nameof(isTriggeredByPostRollStartAndIsSetToZero)} = {isTriggeredByPostRollStartAndIsSetToZero}, {nameof(orderIsAlreadyCancelled)} = {orderIsAlreadyCancelled}, {nameof(isSavedOrder)} = {isSavedOrder}");
				LogMethodCompleted(nameof(DoesOrderStatusNeedsAnUpdate), stopWatch);
				return false;
			}

			LogMethodCompleted(nameof(DoesOrderStatusNeedsAnUpdate), stopWatch);
			return true;
		}

		private void Log(string nameOfMethod, string message, string nameOfObject = null)
		{
			helpers?.Log(nameof(Script), nameOfMethod, message, nameOfObject);
		}

		private void LogMethodStart(string nameOfMethod, out Stopwatch stopwatch, string nameOfObject = null)
		{
			helpers.LogMethodStart(nameof(Script), nameOfMethod, out stopwatch, nameOfObject);
		}

		private void LogMethodCompleted(string nameOfMethod, Stopwatch stopwatch)
		{
			helpers.LogMethodCompleted(nameof(Script), nameOfMethod, null, stopwatch);
		}

		#region IDisposable Support
		private bool disposedValue; // To detect redundant calls

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
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}