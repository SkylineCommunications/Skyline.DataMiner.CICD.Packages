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

16/07/2021	1.0.0.1		TRE, Skyline	Initial version
****************************************************************************
*/

namespace AddEventFromTemplate_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations.Ceiton;
	using Status = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Status;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	public class Script : IDisposable
	{
		private InteractiveController app;

		private UserInfo userInfo;
		private UseEventTemplateDialog useEventTemplateDialog;
		private AddOrUpdateEventDialog eventDialog;
		private EditTemplateDialog editTemplateDialog;
		private AddEventTemplateDialog addEventTemplateDialog;
		private Helpers helpers;

		private System.Threading.Tasks.Task scriptTask;
		private int timeOutResult;
		private bool disposedValue;

		/// <summary>
		/// The Script entry point.
		/// Engine.ShowUI();
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			scriptTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
			{
				try
				{
					Initialize(engine);

					userInfo = helpers.ContractManager.GetUserInfo(engine.UserLoginName);
					Dialog dialog;
					if (userInfo.GetEventTemplates().Any())
					{
						dialog = InitUseEventTemplateDialog();
					}
					else
					{
						MessageDialog messageDialog = new MessageDialog(engine, "You don't have access to any Event Templates.") { Title = "No Templates Available" };
						messageDialog.OkButton.Pressed += (s, a) => engine.ExitSuccess("No Event Templates");

						dialog = messageDialog;
					}

					if (app.IsRunning) app.ShowDialog(dialog);
					else app.Run(dialog);
				}
				catch (ScriptAbortException)
				{
					// Do nothing
				}
				catch (InteractiveUserDetachedException)
				{
					// Do nothing
				}
				catch (Exception e)
				{
					if (timeOutResult != -1)
					{
						helpers.Log(nameof(Script), nameof(Run), "Something went wrong: " + e);
						ShowExceptionDialog(engine, e);
					}
				}
				finally
				{
					Dispose();
				}
			});

			timeOutResult = System.Threading.Tasks.Task.WaitAny(new[] { scriptTask }, new TimeSpan(9, 59, 40));
		}

		private Dialog InitUseEventTemplateDialog()
		{
			var eventTemplates = new List<EventTemplate>();
			foreach (string templateName in userInfo.GetEventTemplates())
			{
				if (!helpers.ContractManager.TryGetEventTemplate(templateName, out EventTemplate template))
				{
					helpers.Log(nameof(Script), nameof(InitUseEventTemplateDialog), $"Unable to retrieve Event Template: {templateName}");
					continue;
				}

				eventTemplates.Add(template);
			}

			useEventTemplateDialog = new UseEventTemplateDialog(helpers, eventTemplates, userInfo);
			useEventTemplateDialog.ContinueButton.Pressed += UseEventTemplateButton_Pressed;
			useEventTemplateDialog.EditSelectedTemplateButton.Pressed += (o, e) => EditTemplate();

			return useEventTemplateDialog;
		}

		private void Initialize(Engine engine)
		{
			engine.SetFlag(RunTimeFlags.NoKeyCaching);
			engine.SetFlag(RunTimeFlags.NoInformationEvents);
			engine.Timeout = TimeSpan.FromHours(10);

			helpers = new Helpers(engine, Scripts.AddEventFromTemplate);

			app = new InteractiveController(engine);
		}

		private void UseEventTemplateButton_Pressed(object sender, EventArgs e)
		{
			if (!useEventTemplateDialog.IsValid) return;

			try
			{
				if (useEventTemplateDialog.EventSubType == EventSubType.Normal)
				{
					var @event = useEventTemplateDialog.SelectedEvent;
					@event.Start = new[] { useEventTemplateDialog.StartTime, @event.Start }.Min();
					@event.End = new[] { useEventTemplateDialog.EndTime, @event.End }.Max();

					LockInfo lockInfo = new LockInfo(true, String.Empty, Guid.Empty.ToString(), TimeSpan.Zero);
					eventDialog = new AddOrUpdateEventDialog(helpers, userInfo, @event, lockInfo);
					eventDialog.SaveAsTemplateButton.Text = "Edit Template";
					eventDialog.SavePreliminaryEventButton.Pressed += (s, a) => SaveEvent(Status.Preliminary);
					eventDialog.SavePlannedEventButton.Pressed += (s, a) => SaveEvent(Status.Planned);
					eventDialog.SaveButton.Pressed += (s, a) => SaveEvent(Status.Confirmed);
					eventDialog.SaveAsTemplateButton.Pressed += (s, a) => EditTemplate();

					app.ShowDialog(eventDialog);
				}
				else
				{
					SaveEvent(Status.Planned);
				}
			}
			catch(Exception ex)
			{
				helpers.Log(nameof(Script), nameof(UseEventTemplateButton_Pressed), $"Exception occurred: {ex}");
				throw;
			}
		}

		private void SaveEvent(Status status)
		{
			try
			{
				Event eventInfo = null;

				if (eventDialog != null)
				{
					if (!eventDialog.IsValid()) return;

					eventInfo = eventDialog.GetUpdatedEvent();
				}
				else if (useEventTemplateDialog != null)
				{
					if (!useEventTemplateDialog.IsValid) return;

					eventInfo = useEventTemplateDialog.SelectedEvent;
				}
				else
				{
					return;
				}

				var progressDialog = new ProgressDialog(helpers.Engine) { Title = "Saving New Event" };
				progressDialog.OkButton.Pressed += (send, args) => helpers.Engine.ExitSuccess(eventInfo.Id.ToString());

				app.ShowDialog(progressDialog);

				eventInfo.Status = status;
				helpers.EventManager.AddOrUpdateEvent(eventInfo);

				// If a project number is defined, the project should be polled from Ceiton.
				// If Ceiton has any information, the Ceiton Integration will update this Event.
				if (!string.IsNullOrWhiteSpace(eventInfo.ProjectNumber))
				{
					var orderManagerElement = new OrderManagerElement(helpers);
					var ceiton = new CeitonElement(orderManagerElement.CeitonElement);
					ceiton.PollProject(eventInfo.ProjectNumber);
				}

				AddOrUpdateLinkedOrders(status, eventInfo, progressDialog);

				progressDialog.Finish();
				app.ShowDialog(progressDialog);
			}
			catch (Exception exception)
			{
				ShowExceptionDialog((Engine)helpers.Engine, exception);
			}
		}

		private void AddOrUpdateLinkedOrders(Status status, Event eventInfo, ProgressDialog progressDialog)
		{
			var linkedOrders = useEventTemplateDialog.SelectedTemplateLinkedOrders.Any() ? useEventTemplateDialog.SelectedTemplateLinkedOrders : useEventTemplateDialog.CreateLinkedOrders(eventInfo);
			foreach (var order in linkedOrders)
			{
				helpers.ProgressReported += (send, args) => progressDialog.AddProgressLine(args.Progress);

				order.Event = eventInfo;
				order.Company = eventInfo.Company;
				order.Contract = eventInfo.Contract;
				order.IsInternal = eventInfo.IsInternal;
				order.Status = useEventTemplateDialog.DetermineOrderStatus(order, status);
				order.AddOrUpdate(helpers, userInfo.IsMcrUser);
			}
		}

		private void EditTemplate()
		{
			Event @event = null;
			Dialog dialogLinkToBackButton = null;
			if (useEventTemplateDialog.EventSubType == EventSubType.Normal)
			{
				if (!eventDialog.IsValid()) return;

				@event = eventDialog.GetUpdatedEvent();
				dialogLinkToBackButton = eventDialog;
			}
			else
			{
				@event = useEventTemplateDialog.SelectedEvent;
				dialogLinkToBackButton = useEventTemplateDialog;
			}

			editTemplateDialog = new EditTemplateDialog((Engine)helpers.Engine);
			editTemplateDialog.BackButton.Pressed += (s, a) => app.ShowDialog(dialogLinkToBackButton);
			editTemplateDialog.CreateNewTemplateButton.Pressed += (s, a) => CreateNewTemplate(@event);
			editTemplateDialog.UpdateTemplateButton.Pressed += (s, a) => UpdateTemplate(@event, useEventTemplateDialog.SelectedTemplate);
			editTemplateDialog.DeleteTemplateButton.Pressed += (s, a) => DeleteTemplate(useEventTemplateDialog.SelectedTemplate);

			app.ShowDialog(editTemplateDialog);
		}

		private void CreateNewTemplate(Event @event)
		{
			addEventTemplateDialog = new AddEventTemplateDialog(helpers, @event, userInfo);
			addEventTemplateDialog.BackButton.Pressed += (s, args) => app.ShowDialog(editTemplateDialog);
			addEventTemplateDialog.SaveTemplateButton.Pressed += (s, args) => SaveNewEventTemplate(@event, addEventTemplateDialog.TemplateName, addEventTemplateDialog.SelectedUserGroups.ToArray());

			app.ShowDialog(addEventTemplateDialog);
		}

		private void SaveNewEventTemplate(Event @event, string templateName, string[] userGroups)
		{
			if (!addEventTemplateDialog.IsValid) return;

			MessageDialog messageDialog;
			if (helpers.ContractManager.TryAddEventTemplate(templateName, userGroups, @event, new Order[0]))
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

		private void UpdateTemplate(Event @event, EventTemplate selectedTemplate)
		{
			addEventTemplateDialog = new AddEventTemplateDialog(helpers, @event, userInfo);
			List<OrderTemplate> linkedOrderTemplates = helpers.ContractManager.GetLinkedOrderTemplates(selectedTemplate.Id);
			addEventTemplateDialog.Init(selectedTemplate, linkedOrderTemplates, userInfo);

			addEventTemplateDialog.BackButton.Pressed += (s, args) => app.ShowDialog(editTemplateDialog);
			addEventTemplateDialog.SaveTemplateButton.Pressed += (s, args) => SaveUpdatedEventTemplate(@event, selectedTemplate, addEventTemplateDialog.TemplateName, addEventTemplateDialog.SelectedUserGroups.ToArray(), addEventTemplateDialog.GetLinkedOrderTemplatesToKeep(), addEventTemplateDialog.NewOrderTemplates, addEventTemplateDialog.NewOrderTemplateOffsets);

			app.ShowDialog(addEventTemplateDialog);
		}

		private void SaveUpdatedEventTemplate(Event @event, EventTemplate selectedTemplate, string templateName, string[] userGroups, IReadOnlyList<OrderTemplate> linkedOrderTemplatesToKeep, IReadOnlyList<OrderTemplate> newOrderTemplates, Dictionary<Guid, TimeSpan> newOrderTemplateOffsets)
		{
			if (!addEventTemplateDialog.IsValid) return;

			MessageDialog messageDialog;
			if (helpers.ContractManager.TryEditEventTemplate(@event, selectedTemplate, templateName, userGroups, linkedOrderTemplatesToKeep, newOrderTemplates, newOrderTemplateOffsets))
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

		private void DeleteTemplate(EventTemplate selectedTemplate)
		{
			MessageDialog messageDialog;
			if (helpers.ContractManager.TryDeleteEventTemplate(selectedTemplate.Name))
			{
				messageDialog = new MessageDialog(helpers.Engine, "The template was successfully removed");
			}
			else
			{
				messageDialog = new MessageDialog(helpers.Engine, "Unable to remove the template");
			}

			messageDialog.OkButton.Pressed += (s, args) => helpers.Engine.ExitSuccess("OK");
			app.ShowDialog(messageDialog);
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			ExceptionDialog dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong during the creation of the new event.");
			if (app.IsRunning) app.ShowDialog(dialog); else app.Run(dialog);
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