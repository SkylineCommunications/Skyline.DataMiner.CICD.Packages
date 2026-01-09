/*
****************************************************************************
*  Copyright (c) 2019,  Skyline Communications NV  All Rights Reserved.    *
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

dd/mm/2019	1.0.0.1		XXX, Skyline	Initial Version
****************************************************************************
*/

using EventStatus = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Status;
using OrderStatus = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status;


//engine.ShowUI();

namespace UpdateEvent_2
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Timers;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Ceiton;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.EventTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Utils.YLE.Integrations.Ceiton;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using Task = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.Task;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private InteractiveController app;

		private LoadAddOrUpdateEventDialog loadAddOrUpdateEventDialog;
		private AddOrUpdateEventDialog editEventDialog;
		private AddEventTemplateDialog addEventTemplateDialog;
		private ProgressDialog progressDialog;
		private Event receivedEvent;
		private Event eventDuplicate;
		private LockInfo lockInfo;
		private Helpers helpers;

		private readonly Timer extendLocksTimer = new Timer();
		private int timeOutResult;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			System.Threading.Tasks.Task scriptTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
			{
				try
				{
					Initialize(engine);

					loadAddOrUpdateEventDialog = new LoadAddOrUpdateEventDialog(helpers, extendLocksTimer);

					if (loadAddOrUpdateEventDialog.Execute()) ShowUpdateEventDialog();
					else app.Run(loadAddOrUpdateEventDialog);
				}
				catch (ScriptAbortException)
				{
					helpers.LockManager.ReleaseLocks();
					extendLocksTimer.Stop();
				}
				catch (InteractiveUserDetachedException)
				{
					helpers.LockManager.ReleaseLocks();
					extendLocksTimer.Stop();
				}
				catch (Exception e)
				{
					if (timeOutResult != -1)
					{
						helpers.LockManager.ReleaseLocks();
						extendLocksTimer.Stop();

						engine.Log("UpdateEvent|Something went wrong|" + e);
						ShowExceptionDialog(engine, e, "Unknown error while editing event");
					}
				}
				finally
				{
					Dispose();
				}
			});

			timeOutResult = System.Threading.Tasks.Task.WaitAny(new[] { scriptTask }, new TimeSpan(9, 59, 40));
			if (timeOutResult == -1)
			{
				helpers.LockManager.ReleaseLocks();
				extendLocksTimer.Stop();
			}
		}

		private void Initialize(Engine engine)
		{
			//engine.ShowUI();
			engine.SetFlag(RunTimeFlags.NoKeyCaching);
			engine.SetFlag(RunTimeFlags.NoInformationEvents);
			engine.Timeout = TimeSpan.FromHours(10);

			helpers = new Helpers(engine, Scripts.UpdateEvent);

			app = new InteractiveController(engine);
		}

		private void ShowUpdateEventDialog()
		{
			receivedEvent = loadAddOrUpdateEventDialog.Event;
			eventDuplicate = loadAddOrUpdateEventDialog.EventDuplicate;
			lockInfo = loadAddOrUpdateEventDialog.LockInfos.Single();

			Dialog dialog;

			editEventDialog = loadAddOrUpdateEventDialog.AddOrUpdateEventDialog;
			editEventDialog.SaveButton.Pressed += (sender, args) => SaveEvent(receivedEvent.Status);
			editEventDialog.SavePlannedEventButton.Pressed += (sender, args) => SaveEvent(EventStatus.Planned);
			editEventDialog.SavePreliminaryEventButton.Pressed += (sender, args) => SaveEvent(EventStatus.Preliminary);
			editEventDialog.CancelButton.Pressed += (sender, args) => ConfirmCancelEvent();
			editEventDialog.SaveAsTemplateButton.Pressed += (sender, args) => ShowAddEventTemplateDialog();
			dialog = editEventDialog;

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}
		private void SaveEvent(EventStatus status)
		{
			try
			{
				if (!editEventDialog.IsValid()) return;

				Event eventInfo = editEventDialog.GetUpdatedEvent();

				var reportDialog = new AddOrUpdateReportDialog(helpers);
				reportDialog.OkButton.Pressed += (o, args) => { helpers.LockManager.ReleaseLocks(); helpers.Engine.ExitSuccess("Successfully updated the service"); };
				app.ShowDialog(reportDialog);

				eventInfo.Status = status;

				// When the event is extended the status should go back to ongoing.
				DateTime now = DateTime.Now;
				if (now > eventDuplicate?.End && now < receivedEvent?.End) eventInfo.Status = EventStatus.Ongoing;

				bool isInternalPropertyHasChanged = eventDuplicate?.IsInternal != eventInfo.IsInternal;
				bool eventContainsOrders = helpers.EventManager.HasOrders(eventInfo.Id);
				bool nameHasChanged = (eventDuplicate?.Name != eventInfo.Name);
				bool orderUpdateRequired = eventContainsOrders && (isInternalPropertyHasChanged || nameHasChanged);


				List<Task> tasks = new List<Task>();
				var addOrUpdateEventTask = new AddOrUpdateEventTask(helpers, eventInfo);
				tasks.Add(addOrUpdateEventTask);

				if (!addOrUpdateEventTask.Execute())
				{
					helpers.Log(nameof(Script), nameof(SaveEvent), $"Add or update event task failed for event: {eventInfo.Name}");
				}

				if (orderUpdateRequired)
				{
					helpers.ReportProgress("Getting orders linked to event...");
					var ordersInEvent = helpers.EventManager.GetOrdersInEvent(eventInfo.Id);
					helpers.ReportProgress("Getting orders linked to event succeeded");

					foreach (var order in ordersInEvent)
					{
						helpers.ReportProgress("Updating linked order " + order.Name + " ...");
						var propertiesToBeChanged = new Dictionary<string, object>()
						{
							{ LiteOrder.PropertyNameInternal, eventInfo.IsInternal.ToString() },
							{ LiteOrder.PropertyNameEventName, eventInfo.Name }
						};
						order.TryUpdateCustomProperties(helpers, propertiesToBeChanged);
						helpers.ReportProgress("Updating linked order " + order.Name + " succeeded");
					}
				}

				// If a project number is defined, the project should be polled from Ceiton.
				// If Ceiton has any information, the Ceiton Integration will update this Event.
				if (!String.IsNullOrWhiteSpace(eventInfo.ProjectNumber))
				{
					var orderManagerElement = new OrderManagerElement(helpers);
					var ceiton = new CeitonElement(orderManagerElement.CeitonElement);
					ceiton.PollProject(eventInfo.ProjectNumber);
				}

				reportDialog.Finish(tasks);
				app.ShowDialog(reportDialog);
			}
			catch (Exception exception)
			{
				ShowExceptionDialog((Engine)helpers.Engine, exception, "Something went wrong while updating the event");
			}
		}

		private void ConfirmCancelEvent()
		{
			bool containsActiveOrders = helpers.EventManager.GetOrdersInEvent(receivedEvent.Id).Any(o => o.Status != OrderStatus.Cancelled);
			CancelEventDialog dialog = new CancelEventDialog(helpers.Engine, lockInfo, receivedEvent.Status == EventStatus.Ongoing, containsActiveOrders);
			dialog.YesButton.Pressed += (sender, args) => CancelEvent();
			dialog.NoButton.Pressed += (sender, args) => { app.ShowDialog(editEventDialog); };

			app.ShowDialog(dialog);
		}

		private void CancelEvent()
		{
			try
			{
				bool success = true;

				// Cancel active Orders
				foreach (Order order in helpers.EventManager.GetOrdersInEvent(receivedEvent.Id))
				{
					if (order.Status != OrderStatus.Cancelled)
					{
						success &= order.UpdateStatus(helpers, OrderStatus.Cancelled);
					}
				}

				if (!success)
				{
					ShowMessageDialog((Engine)helpers.Engine, "Unable to cancel event", "Unable to cancel some active orders that are part of the event.");
					return;
				}

				// Cancel Event
				if (!helpers.EventManager.UpdateEventStatus(receivedEvent.Id, EventStatus.Cancelled))
				{
					ShowMessageDialog((Engine)helpers.Engine, "Unable to cancel event", String.Format("Event {0} could not be canceled", receivedEvent.Name));
					return;
				}

				helpers.Engine.ExitSuccess("The event was successfully canceled.");
			}
			catch (ScriptAbortException)
			{
				throw;
			}
			catch (Exception exception)
			{
				ShowExceptionDialog((Engine)helpers.Engine, exception, "Something went wrong while canceling the event");
			}
		}

		private void ShowAddEventTemplateDialog()
		{
			var @event = editEventDialog.GetUpdatedEvent();
			var userInfo = loadAddOrUpdateEventDialog.UserInfo;

			addEventTemplateDialog = new AddEventTemplateDialog(helpers, @event, userInfo);
			addEventTemplateDialog.BackButton.Pressed += (s, args) => app.ShowDialog(editEventDialog);
			addEventTemplateDialog.SaveTemplateButton.Pressed += (s, args) => SaveEventTemplate();

			app.ShowDialog(addEventTemplateDialog);
		}

		private void SaveEventTemplate()
		{
			if (!addEventTemplateDialog.IsValid) return;

			progressDialog = new ProgressDialog(helpers.Engine);
			app.ShowDialog(progressDialog);

			// Get underlying orders that are not integration orders
			Event @event = addEventTemplateDialog.Event;
			OrderManager orderManager = new OrderManager(helpers);
			List<Order> underlyingOrders = new List<Order>();
			foreach (Guid orderId in @event.OrderIds)
			{
				if (orderId == Guid.Empty) continue;
				if (@event.OrderIsIntegrations.ContainsKey(orderId) && @event.OrderIsIntegrations[orderId]) continue;

				progressDialog.AddProgressLine($"Retrieving Order with ID: {orderId}...");

				Order order = orderManager.GetOrder(orderId);
				if (order == null) continue;

				underlyingOrders.Add(order);
			}

			string eventTemplateName = addEventTemplateDialog.TemplateName;
			string[] userGroups = addEventTemplateDialog.SelectedUserGroups.ToArray();

			// Save templates
			progressDialog.AddProgressLine($"Saving Event Template...");
			MessageDialog messageDialog;
			if (helpers.ContractManager.TryAddEventTemplate(eventTemplateName, userGroups, @event, underlyingOrders))
			{
				messageDialog = new MessageDialog(helpers.Engine, "The template was successfully saved");
			}
			else
			{
				messageDialog = new MessageDialog(helpers.Engine, "Unable to save the template");
			}

			messageDialog.OkButton.Pressed += (s, args) => helpers.Engine.ExitSuccess("OK");
			app.ShowDialog(messageDialog);
		}

		private void ShowExceptionDialog(Engine engine, Exception exception, string message)
		{
			ExceptionDialog dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess(message);

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}

		private void ShowMessageDialog(Engine engine, string title, string message)
		{
			MessageDialog messageDialog = new MessageDialog(engine, message) { Title = title };
			messageDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess(message);

			if (app.IsRunning) app.ShowDialog(messageDialog);
			else app.Run(messageDialog);
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					helpers?.Dispose();
					extendLocksTimer.Dispose();
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