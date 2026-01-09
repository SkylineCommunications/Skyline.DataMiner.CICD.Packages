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

26/05/2020	1.0.0.1		TRE, Skyline	Initial version
****************************************************************************
*/



//---------------------------------
// MergeEvents_2.cs
//---------------------------------

namespace MergeEvents_2
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Timers;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using Task = System.Threading.Tasks.Task;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	class Script : IDisposable
	{
		private InteractiveController app;
		private readonly Timer extendLocksTimer = new Timer();

		private LoadMergeEventsDialog loadMergeEventsDialog;
		private MergeEventsDialog mergeEventsDialog;
		private ProgressDialog progressDialog;
		private Helpers helpers;

		private int timeOutResult;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			var scriptTask = Task.Factory.StartNew(() =>
			{
				try
				{
					var stopwatch = new Stopwatch();
					stopwatch.Start();

					Initialize(engine);

					loadMergeEventsDialog = new LoadMergeEventsDialog(helpers, extendLocksTimer);

					stopwatch.Stop();
					engine.Log(nameof(Script), nameof(Run), $"Finished constructing dialog [{stopwatch.Elapsed}]");

					if (loadMergeEventsDialog.Execute()) ShowMergeEventsDialog();
					else app.Run(loadMergeEventsDialog);
				}
				catch (ScriptAbortException)
				{
					helpers.LockManager.ReleaseLocks();
				}
				catch (InteractiveUserDetachedException)
				{
					helpers.LockManager.ReleaseLocks();
				}
				catch (Exception e)
				{
					if (timeOutResult != -1)
					{
						helpers.LockManager.ReleaseLocks();
						ShowExceptionDialog(engine, e);
					}
				}
				finally
				{
					Dispose();
				}
			});

			timeOutResult = Task.WaitAny(new[] { scriptTask }, new TimeSpan(9, 59, 40));
			if (timeOutResult == -1)
			{
				helpers.LockManager.ReleaseLocks();
				Dispose();
			}
		}

		private void Initialize(Engine engine)
		{
			engine.SetFlag(RunTimeFlags.NoKeyCaching);
			engine.SetFlag(RunTimeFlags.NoCheckingSets);
			engine.SetFlag(RunTimeFlags.NoInformationEvents);
			engine.Timeout = TimeSpan.FromHours(10);
			// engine.ShowUI();

			app = new InteractiveController(engine);
			helpers = new Helpers(engine, Scripts.MergeEvents);
		}

		private void ShowMergeEventsDialog()
		{
			mergeEventsDialog = loadMergeEventsDialog.MergeEventsDialog;
			mergeEventsDialog.MergeEventsButton.Pressed += MergeEventsButton_Pressed;

			if (app.IsRunning)app.ShowDialog(mergeEventsDialog);
			else app.Run(mergeEventsDialog);
		}

		private void MergeEventsButton_Pressed(object sender, EventArgs e)
		{
			var confirmationDialog = new ConfirmationDialog((Engine)helpers.Engine, mergeEventsDialog.GetEventsToRemove(), mergeEventsDialog.GetOrdersToRemove());
			confirmationDialog.BackButton.Pressed += (s, args) => app.ShowDialog(mergeEventsDialog);
			confirmationDialog.ContinueButton.Pressed += ContinueButton_Pressed;
			app.ShowDialog(confirmationDialog);
		}

		private void ContinueButton_Pressed(object sender, EventArgs e)
		{
			progressDialog = new ProgressDialog(helpers.Engine);
			progressDialog.OkButton.Pressed += (s, args) => helpers.Engine.ExitSuccess(mergeEventsDialog.PrimaryEvent.Id.ToString());
			app.ShowDialog(progressDialog);

			MergeEvents();
		
			progressDialog.Finish();
			app.ShowDialog(progressDialog);
		}

		private void MergeEvents()
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			MoveOrdersToPrimaryEvent();

			DeleteOrders();

			DeleteEvents();

			stopwatch.Stop();

			progressDialog.AddProgressLine("Successfully merged the events");
		}

		private void MoveOrdersToPrimaryEvent()
		{
			progressDialog.AddProgressLine("Moving orders to the primary event...");

			MoveOrders();

			MoveIntegrationOrders();
		}

		private void MoveOrders()
		{
			foreach (var order in mergeEventsDialog.GetNonIntegrationOrdersToMove())
			{
				try
				{
					progressDialog.AddProgressLine($"Moving order {order.Name} to event {mergeEventsDialog.PrimaryEvent.Name}...");

					bool moveSucceeded = helpers.EventManager.AddOrUpdateOrderToEvent(mergeEventsDialog.PrimaryEvent.Id, order, orderEventReferenceUpdateRequired: true);

					progressDialog.AddProgressLine($"Moving order {order.Name} to event {mergeEventsDialog.PrimaryEvent.Name} {(moveSucceeded ? "succeeded" : "failed")}");

					helpers.OrderManager.UpdateEventNameProperty(order.Id, mergeEventsDialog.PrimaryEvent.Name);
					//// Update event in order
					//order.Event = mergeEventsDialog.PrimaryEvent;
					//order.UpdateEventReference(helpers);
				}
				catch (Exception e)
				{
					progressDialog.AddProgressLine($"Moving order {order.Name} to event {mergeEventsDialog.PrimaryEvent.Name} failed");
					helpers.Log(nameof(Script), nameof(MoveOrders), $"Moving order {order.Name} to event {mergeEventsDialog.PrimaryEvent.Name} failed: {e}");
				}
			}
		}

		private void MoveIntegrationOrders()
		{
			var movedOrderIds = mergeEventsDialog.GetNonIntegrationOrdersToMove().Select(o => o.Id).ToList();

			foreach (var @event in mergeEventsDialog.Events)
			{
				if (@event == mergeEventsDialog.PrimaryEvent) continue;

				foreach (var orderId in @event.OrderIds)
				{
					bool orderIsAlreadyMoved = movedOrderIds.Contains(orderId);
					if (orderIsAlreadyMoved) continue;

					progressDialog.AddProgressLine($"Getting order ...");

					var order = helpers.OrderManager.GetLiteOrder(orderId);

					progressDialog.AddProgressLine($"Getting order {order.Name} succeeded");

					try
					{
						progressDialog.AddProgressLine($"Moving order {order.Name} to event {mergeEventsDialog.PrimaryEvent.Name}...");

						bool moveSucceeded = helpers.EventManager.AddOrUpdateOrderToEvent(mergeEventsDialog.PrimaryEvent.Id, order, orderEventReferenceUpdateRequired: true);
						helpers.OrderManager.UpdateEventNameProperty(orderId, mergeEventsDialog.PrimaryEvent.Name);
						progressDialog.AddProgressLine($"Moving order {order.Name} to event {mergeEventsDialog.PrimaryEvent.Name} {(moveSucceeded ? "succeeded" : "failed")}");
					}
					catch (Exception e)
					{
						progressDialog.AddProgressLine($"Moving order {order.Name} to event {mergeEventsDialog.PrimaryEvent.Name} failed");
						helpers.Log(nameof(Script), nameof(MoveIntegrationOrders), $"Moving order {order.Name} to event {mergeEventsDialog.PrimaryEvent.Name} failed: {e}");
					}
				}
			}
		}

		private void DeleteOrders()
		{
			var ordersToRemove = mergeEventsDialog.GetOrdersToRemove();
			if (!ordersToRemove.Any()) return;

			progressDialog.AddProgressLine("Deleting orders...");

			var orderIdsToRemove = ordersToRemove.Select(order => order.Id).ToArray();

			helpers.AddOrderReferencesForLogging(orderIdsToRemove);

			foreach (var orderToRemove in ordersToRemove)
			{
				try
				{
					progressDialog.AddProgressLine($"Removing order {orderToRemove.Name}...");
					var tasks = helpers.OrderManager.DeleteOrder(orderToRemove.Id);

					progressDialog.AddProgressLine($"Removing order {orderToRemove.Name} {(tasks.Any(t => t.Status == Status.Fail) ? "failed" : "succeeded")}");
				}
				catch (Exception e)
				{
					progressDialog.AddProgressLine($"Removing order {orderToRemove.Name} failed");
					helpers.Log(nameof(Script), nameof(DeleteOrders), $"MergeEvents|Removing order {orderToRemove.Name} failed|{e}", orderToRemove.Name);
				}
			}
		}

		private void DeleteEvents()
		{
			var eventsToRemove = mergeEventsDialog.GetEventsToRemove();
			if (!eventsToRemove.Any()) return;

			progressDialog.AddProgressLine("Deleting events...");

			var primaryEvent = mergeEventsDialog.PrimaryEvent;
			foreach (var eventToRemove in eventsToRemove)
			{
				if (eventToRemove == null) continue;
			
				// Move Attachments
				if (helpers.EventManager.HasAttachments(eventToRemove.Id) && primaryEvent != null)
				{
					progressDialog.AddProgressLine($"Moving attachments from {eventToRemove.Name} to {primaryEvent.Name}...");

					bool moveSucceeded = helpers.EventManager.MoveAttachments(eventToRemove.Id, primaryEvent.Id);

					progressDialog.AddProgressLine($"Moving attachments from {eventToRemove.Name} to {primaryEvent.Name} {(moveSucceeded ? "succeeded" : "failed")}");
				}

				try
				{
					// Delete event
					progressDialog.AddProgressLine($"Removing event {eventToRemove.Name}...");

					bool deleteSucceeded = helpers.EventManager.DeleteEvent(eventToRemove.Id);

					progressDialog.AddProgressLine($"Removing event {eventToRemove.Name} {(deleteSucceeded ? "succeeded" : "failed")}");
				}
				catch (Exception e)
				{
					progressDialog.AddProgressLine($"Removing event {eventToRemove.Name} failed");
					helpers.Log(nameof(Script), nameof(DeleteEvents), $"Removing event {eventToRemove.Name} failed: {e}");
				}
			}
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			ExceptionDialog dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong. See the logs for more information.");
			if(app.IsRunning)
			{
				app.ShowDialog(dialog);
			}
			else
			{
				app.Run(dialog);
			}
		}

		#region IDisposable Support
		private bool disposedValue; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					extendLocksTimer.Stop();
					extendLocksTimer.Dispose();
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
//---------------------------------
// DialogsAndSections\ConfirmationDialog.cs
//---------------------------------