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

18/03/2021	1.0.0.1		TRE, Skyline	Updated edit rights for Non-MCR users
06/04/2021	1.0.0.2		TRE, Skyline	Allow multiple file selection.
****************************************************************************
*/

namespace UpdateNonLiveUserTask_1
{
	using System;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using Type = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.IngestExport.Type;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	internal class Script : IDisposable
	{
		private InteractiveController app;

		private LoadNonLiveUserTaskDialog loadNonLiveUserTaskDialog;
		private NonLiveUserTaskDialog nonLiveUserTaskDialog;
		private Helpers helpers;

		private int timeOutResult;
		private bool disposedValue;

		public void Run(Engine engine)
		{
			var scriptTask = Task.Factory.StartNew(() =>
			{
				try
				{
					Initialize(engine);

					loadNonLiveUserTaskDialog = new LoadNonLiveUserTaskDialog(helpers);

					if (loadNonLiveUserTaskDialog.Execute()) ShowNonLiveUserTaskForm();
					else app.Run(loadNonLiveUserTaskDialog);
				}
				catch (InteractiveUserDetachedException)
				{
					Dispose();
				}
				catch (ScriptAbortException)
				{
					Dispose();
				}
				catch (Exception e)
				{
					if (timeOutResult != -1)
					{
						engine.Log("Run|Something went wrong: " + e);
						ShowExceptionDialog(e);
					}
				}
				finally
				{
					Dispose();
				}
			});

			timeOutResult = Task.WaitAny(new[] { scriptTask }, new TimeSpan(9, 59, 40));
		}

		private void Initialize(Engine engine)
		{
			engine.Timeout = TimeSpan.FromHours(10);
			// engine.ShowUI();

			helpers = new Helpers(engine, Scripts.UpdateNonLiveUserTask);
			app = new InteractiveController(engine);
		}

		private void ShowNonLiveUserTaskForm()
		{
			nonLiveUserTaskDialog = loadNonLiveUserTaskDialog.NonLiveUserTaskDialog;
			nonLiveUserTaskDialog.CloseButton.Pressed += (o, e) => helpers.Engine.ExitSuccess("Non Live User Task Dialog Closed");
			nonLiveUserTaskDialog.ConfirmChangesButton.Pressed += ConfirmChangesButton_Pressed;

			if (app.IsRunning) app.ShowDialog(nonLiveUserTaskDialog);
			else app.Run(nonLiveUserTaskDialog);
		}

		private void ConfirmChangesButton_Pressed(object sender, Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets.YleValueWidgetChangedEventArgs e)
		{
			nonLiveUserTaskDialog.UpdateUserTask();

			ShowMessageDialog("Update succeeded", "Update User Task");
		}

		private void ShowExceptionDialog(Exception exception)
		{
			ExceptionDialog exceptionDialog = new ExceptionDialog((Engine)helpers.Engine, exception);
			exceptionDialog.OkButton.Pressed += (sender, args) => helpers.Engine.ExitSuccess("Something went wrong during update of a user task");
			if (app.IsRunning) app.ShowDialog(exceptionDialog); else app.Run(exceptionDialog);
		}

		private void ShowMessageDialog(string message, string title)
		{
			MessageDialog dialog = new MessageDialog(helpers.Engine, message) { Title = title };
			dialog.OkButton.Pressed += (sender, args) => helpers.Engine.ExitSuccess(message);

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
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