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

namespace AddEvent_2
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Timers;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.EventTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.YLE.Integrations.Ceiton;
	using Status = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event.Status;
	using Task = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.Task;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : YleInteractiveScript
	{
		private LoadAddOrUpdateEventDialog loadAddOrUpdateEventDialog;
		private AddOrUpdateEventDialog eventDialog;
		private AddEventTemplateDialog addEventTemplateDialog;

		private readonly Timer extendLocksTimer = new Timer();

		protected override Scripts ScriptName => Scripts.AddEvent;

		protected override TimeSpan? TimeOut => TimeSpan.FromHours(9);

		protected override void InternalRun()
		{
			loadAddOrUpdateEventDialog = new LoadAddOrUpdateEventDialog(helpers, extendLocksTimer);

			if (loadAddOrUpdateEventDialog.Execute()) ShowAddEventDialog();
			else app.Run(loadAddOrUpdateEventDialog);		
		}

		protected override void EngineShowUiInComments()
		{
			//engine.ShowUI()
		}

		private void ShowAddEventDialog()
		{
			eventDialog = loadAddOrUpdateEventDialog.AddOrUpdateEventDialog;
			eventDialog.SavePreliminaryEventButton.Pressed += (sender, args) => SaveEvent(Status.Preliminary);
			eventDialog.SavePlannedEventButton.Pressed += (sender, args) => SaveEvent(Status.Planned);
			eventDialog.SaveAsTemplateButton.Pressed += (sender, args) => ShowAddEventTemplateDialog();

			if (app.IsRunning) app.ShowDialog(eventDialog);
			else app.Run(eventDialog);
		}

		private void SaveEvent(Status status)
		{
			try
			{
				if (!eventDialog.IsValid()) return;

				var eventInfo = eventDialog.GetUpdatedEvent();

				var reportDialog = new AddOrUpdateReportDialog(helpers);
				reportDialog.OkButton.Pressed += (o, args) => { helpers.LockManager.ReleaseLocks(); helpers.Engine.ExitSuccess("Successfully updated the service"); };
				app.ShowDialog(reportDialog);

				eventInfo.Status = status;

				List<Task> tasks = new List<Task>();
				var addOrUpdateEventTask = new AddOrUpdateEventTask(helpers, eventInfo);
				tasks.Add(addOrUpdateEventTask);

				if (!addOrUpdateEventTask.Execute())
				{
					helpers.Log(nameof(AddEvent_2.Script), nameof(SaveEvent), $"Add or update event task failed for event: {eventInfo.Name}");
				}

				// If a project number is defined, the project should be polled from Ceiton.
				// If Ceiton has any information, the Ceiton Integration will update this Event.
				if (!string.IsNullOrWhiteSpace(eventInfo.ProjectNumber))
				{
					OrderManagerElement orderManagerElement = new OrderManagerElement(helpers);
					var ceiton = new CeitonElement(orderManagerElement.CeitonElement);
					ceiton.PollProject(eventInfo.ProjectNumber);
				}

				reportDialog.Finish(tasks);
				app.ShowDialog(reportDialog);
			}
			catch (Exception exception)
			{
				ShowExceptionDialog(exception);
			}
		}

		protected override void HandleException(Exception e)
		{
			ShowExceptionDialog(e);
		}

		private void ShowAddEventTemplateDialog()
		{
			var @event = eventDialog.GetUpdatedEvent();
			var userInfo = loadAddOrUpdateEventDialog.UserInfo;

			addEventTemplateDialog = new AddEventTemplateDialog(helpers, @event, userInfo);
			addEventTemplateDialog.BackButton.Pressed += (s, args) => app.ShowDialog(eventDialog);
			addEventTemplateDialog.SaveTemplateButton.Pressed += (s, args) => SaveEventTemplate();

			app.ShowDialog(addEventTemplateDialog);
		}

		private void SaveEventTemplate()
		{
			if (!addEventTemplateDialog.IsValid) return;

			string templateName = addEventTemplateDialog.TemplateName;
			string[] userGroups = addEventTemplateDialog.SelectedUserGroups.ToArray();
			Event @event = addEventTemplateDialog.Event;

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

		#region IDisposable Support
		private bool isDisposed;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (!isDisposed)
			{
				if (disposing)
				{
					extendLocksTimer.Dispose();
				}

				isDisposed = true;
			}
		}
		#endregion
	}
}