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

22/11/2021	1.0.0.1		TRE, Skyline	Initial version
****************************************************************************
*/

namespace ConfirmOrders_5
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notifications;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private Helpers helpers;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(Engine engine)
		{
			try
			{
				helpers = new Helpers(engine, Scripts.ConfirmOrders);

				var stopwatch = Stopwatch.StartNew();

				var orderReservation = GetOrderReservationInstance(engine);
				if (orderReservation != null)
				{
					Order order = helpers.OrderManager.GetOrder(orderReservation, forceServiceReservationsToOverwriteServiceConfig: false, skipGettingEvent: true);

					if (orderReservation.Start <= DateTime.Now.ToUniversalTime())
					{
						// Order is running
						helpers.Log(nameof(Script), nameof(Run), "Confirming Running Order");
						UpdateRunningOrderStatus(helpers, order, orderReservation);
					}
					else
					{
						// Order is not running
						helpers.Log(nameof(Script), nameof(Run), "Confirming Non-Running Order");
						UpdateOrderStatus(helpers, order, orderReservation);
					}
				}

				stopwatch.Stop();
				helpers.Log(nameof(Script), nameof(Run), $"Confirming Order took {stopwatch.ElapsedMilliseconds} ms");
			}
			catch (ScriptAbortException)
			{
				Dispose();
			}
			catch (InteractiveUserDetachedException)
			{
				Dispose();
			}
			catch (Exception e)
			{
				helpers?.Log(nameof(Script), nameof(Run), $"Something went wrong: {e}");
				Dispose();
			}
		}

		private ServiceReservationInstance GetOrderReservationInstance(Engine engine)
		{
			if (!TryGetOrderId(engine, out var orderId))
			{
				helpers.Log(nameof(Script), nameof(UpdateOrderStatus), $"Invalid booking id: {engine.GetScriptParam("BookingID")?.Value}");
				return null;
			}

			var orderReservation = DataMinerInterface.ResourceManager.GetReservationInstance(helpers, orderId) as ServiceReservationInstance;
			if (orderReservation == null)
			{
				helpers.Log(nameof(Script), nameof(UpdateOrderStatus), $"No reservation instance found with ID: {orderId}");
				return null;
			}

			helpers.AddOrderReferencesForLogging(orderReservation.ID);

			return orderReservation;
		}

		private static void UpdateRunningOrderStatus(Helpers helper, Order order, ReservationInstance reservation)
		{
			if (order == null)
			{
				helper.Log(nameof(Script), nameof(UpdateRunningOrderStatus), $"No Order found with ID: {reservation.ID}");
				return;
			}

			order.UpdateStatus(helper, Status.Confirmed);
			order.UpdateUiProperties(helper);
		}

		private static void UpdateOrderStatus(Helpers helper, Order order, ServiceReservationInstance reservationInstance)
		{
			if (order == null)
			{
				helper.Log(nameof(Script), nameof(UpdateOrderStatus), $"No Order found with ID: {reservationInstance.ID}");
				return;
			}

			order.Status = Status.Confirmed;
			try
			{
				DataMinerInterface.ReservationInstance.UpdateServiceReservationProperties(helper, reservationInstance, new Dictionary<string, object> { { LiteOrder.PropertyNameStatus, Status.Confirmed.GetDescription() } });
				helper.Log(nameof(Script), nameof(UpdateOrderStatus), $"Updated status of order {reservationInstance.ID} to Confirmed");
			}
			catch(Exception e)
			{
				helper.Log(nameof(Script), nameof(UpdateOrderStatus), $"Unable to update status of order {reservationInstance.ID} to Confirmed|{e}");
			}

			order.UpdateUiProperties(helper);
			NotificationManager.SendLiveOrderConfirmationMail(helper, order);
		}

		private static bool TryGetOrderId(IEngine engine, out Guid orderId)
		{
			orderId = Guid.Empty;
			string bookingId = engine.GetScriptParam("BookingID")?.Value;

			if (String.IsNullOrEmpty(bookingId)) return false;
			if (!Guid.TryParse(bookingId, out orderId)) return false;
			return true;
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
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}