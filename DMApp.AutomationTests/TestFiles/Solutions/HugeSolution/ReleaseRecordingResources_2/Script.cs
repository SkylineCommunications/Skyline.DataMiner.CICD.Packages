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

13/04/2021	1.0.0.1		GVH, Skyline	Initial version
****************************************************************************
*/

namespace ReleaseRecordingResources_2
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Functions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private Helpers helpers;
		private InteractiveController app;

		private LoadAssignReleaseResourceDialog loadAssignReleaseResourceDialog;
		private AssignReleaseResourceDialog releaseResourceDialog;
		private ReportDialog reportDialog;
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

				loadAssignReleaseResourceDialog = new LoadAssignReleaseResourceDialog(helpers, ResourceScriptAction.Release);

				if (loadAssignReleaseResourceDialog.Execute()) ShowReleaseResourceDialog();
				else app.Run(loadAssignReleaseResourceDialog);
			}
			catch (InteractiveUserDetachedException)
			{
				helpers.Log(nameof(Script), "STOP SCRIPT", "Release Recording Resource");
				helpers.Dispose();
				helpers.LockManager.ReleaseLocks();
			}
			catch (ScriptAbortException)
			{
				helpers.Log(nameof(Script), "STOP SCRIPT", "Release Recording Resource");
				helpers.Dispose();
				helpers.LockManager.ReleaseLocks();
				throw;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), "STOP SCRIPT", "Release Recording Resource");
				helpers.Dispose();
				engine.Log("Run|Something went wrong: " + e);
				ShowExceptionDialog(engine, e);
			}
		}

		private void Initialize(Engine engine)
		{
			// engine.ShowUI();
			engine.Timeout = TimeSpan.FromHours(10);
			engine.SetFlag(RunTimeFlags.NoCheckingSets);
			engine.SetFlag(RunTimeFlags.NoInformationEvents);
			engine.SetFlag(RunTimeFlags.NoKeyCaching);

			helpers = new Helpers(engine, Scripts.ReleaseRecordingResources);

			app = new InteractiveController(engine);
		}

		private void ShowReleaseResourceDialog()
		{
			releaseResourceDialog = loadAssignReleaseResourceDialog.AssignReleaseResourceDialog;
			releaseResourceDialog.ReleaseButton.Pressed += ReleaseButton_Pressed;

			if (app.IsRunning) app.ShowDialog(releaseResourceDialog);
			else app.Run(releaseResourceDialog);
		}

		private void ReleaseButton_Pressed(object sender, EventArgs e)
		{
			ConfirmReleaseDialog confirmReleaseDialog = new ConfirmReleaseDialog((Engine)helpers.Engine);
			confirmReleaseDialog.NoButton.Pressed += (o, args) => app.ShowDialog(releaseResourceDialog);
			confirmReleaseDialog.YesButton.Pressed += ConfirmReleaseDialog_YesButton_Pressed;

			app.ShowDialog(confirmReleaseDialog);
		}

		private void ConfirmReleaseDialog_YesButton_Pressed(object sender, EventArgs e)
		{
			try
			{
				reportDialog = new AddOrUpdateReportDialog(helpers);
				reportDialog.OkButton.Pressed += (o, args) => { helpers.LockManager.ReleaseLocks(); helpers.Engine.ExitSuccess("Successfully updated the recording service"); };
				reportDialog.RollBackButton.Pressed += (o, args) => RollBackTasks();
				app.ShowDialog(reportDialog);

				var tasks = releaseResourceDialog.Finish().Tasks.ToList();

				if (tasks.Any())
				{
					reportDialog.Finish(tasks);

					// release the locks if all tasks were successful
					if (tasks.All(t => t.Status == Status.Ok)) helpers.LockManager.ReleaseLocks();

					app.ShowDialog(reportDialog);
				}
				else ShowMessageDialog((Engine)helpers.Engine, "No changes made", "No Changes made");

			}
			catch (Exception ex)
			{
				ShowExceptionDialog((Engine)helpers.Engine, ex);
			}
		}

		private void RollBackTasks()
		{
			// Roll back tasks - Only roll back tasks that were successful
			var rollbackReportDialog = new RollBackReportDialog(helpers) { Title = "Roll back service update" };
			rollbackReportDialog.OkButton.Pressed += (o, e) => helpers.Engine.ExitSuccess("Successfully rolled back the service update");

			app.ShowDialog(rollbackReportDialog);

			// Only roll back tasks that were successful
			List<Task> rollbackTasks = reportDialog.UpdateResults.SelectMany(ur => ur.Tasks).Where(t => t.Status == Status.Ok).Select(t => t.CreateRollbackTask()).Where(t => t != null).Reverse().ToList();

			foreach (Task rollbackTask in rollbackTasks)
			{
				if (!rollbackTask.Execute())
				{
					helpers.Log(nameof(Script), nameof(RollBackTasks), "Rolling back " + rollbackTask.Description + "failed: " + rollbackTask.Exception);
					break;
				}
			}

			rollbackReportDialog.Finish(rollbackTasks);

			helpers.LockManager.ReleaseLocks();

			app.ShowDialog(rollbackReportDialog);
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
			ExceptionDialog dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitFail("Something went wrong during the handling of resource assigning");
			if (!app.IsRunning) app.Run(dialog);
			else app.ShowDialog(dialog);
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