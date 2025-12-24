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

22/03/2021	1.0.0.2		GVH, Skyline	Linking between user tasks and non live orders 
****************************************************************************
*/

namespace AssignTicketToMe_2
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	class Script : IDisposable
	{
		private Helpers helpers;
		private InteractiveController app;
		private User user;
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

				string scriptParameter = engine.GetScriptParam(1).Value;

				int[] dmaAndTicketId = Array.ConvertAll(scriptParameter.Split(new[] { '/' }).Select(item => item.Trim('"', '[', ']')).ToArray(), Convert.ToInt32);

				if (helpers.NonLiveOrderManager.TryGetNonLiveOrder(dmaAndTicketId[0], dmaAndTicketId[1], out var nonLiveOrder))
				{
					helpers.NonLiveOrderManager.AssignNonLiveOrderTo(nonLiveOrder, user);
				}
				else if (helpers.UserTaskManager.TryGetUserTask(scriptParameter, out var userTask))
				{
					userTask.Owner = engine.UserDisplayName;
					userTask.AddOrUpdate(helpers);

					bool assignToMe = true;
					if (userTask is NonLiveUserTask nonLiveUserTask && !helpers.NonLiveOrderManager.TryUpdateNonLiveOrderStatusBasedOnUserTask(helpers.NonLiveUserTaskManager, nonLiveUserTask, user, assignToMe))
					{
						helpers.Log(nameof(Script), nameof(Run), $"Non live order failed while updating the status");
					}
				}
				else
				{
					helpers.Log(nameof(Script), nameof(Run), $"Ticket with ID {scriptParameter} is not a non-live order nor a user task.");

					ShowMessageDialog(engine, $"Assigning the ticket to you failed because the system was unable to convert the ticket with ID {scriptParameter} to a non-live order or user task.", "Error");
				}
			}
			catch (ScriptAbortException)
			{
				// ignore
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Something went wrong: {e}");
				ShowExceptionDialog(engine, e);
			}
			finally
			{
				helpers.Log(nameof(Script), "STOP SCRIPT", "AssignTicketToMe");
				Dispose();
			}
		}

		private void Initialize(Engine engine)
		{
			//engine.ShowUI();
			helpers = new Helpers(engine, Scripts.AssignTicketToMe);

			app = new InteractiveController(engine);

			user = helpers.ContractManager.GetBaseUserInfo(engine.UserLoginName).User;
		}

		private void ShowMessageDialog(Engine engine, string message, string title)
		{
			MessageDialog dialog = new MessageDialog(engine, message) { Title = title };
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess(message);

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			ExceptionDialog exceptionDialog = new ExceptionDialog(engine, exception);
			exceptionDialog.OkButton.Pressed += (o, e) => engine.ExitSuccess("Something went wrong during the execution of Assign Ticket to me");

			if (app.IsRunning) app.ShowDialog(exceptionDialog);
			else app.Run(exceptionDialog);
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