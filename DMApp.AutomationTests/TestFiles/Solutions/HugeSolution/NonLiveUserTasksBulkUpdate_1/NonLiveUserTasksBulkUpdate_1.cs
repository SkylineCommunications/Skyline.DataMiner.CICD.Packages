/*
****************************************************************************
*  Copyright (c) 2023,  Skyline Communications NV  All Rights Reserved.    *
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

dd/mm/2023	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

namespace NonLiveUserTasksBulkUpdate_1
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script : IDisposable
	{
		private Helpers helpers;
		private InteractiveController dialogController;

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			Initialize(engine);

			try
			{
				var loadingDialog = new NonLiveUserTasksBulkUpdateLoadingDialog(helpers);

				if (loadingDialog.Execute()) ShowNonLiveUserTasksBulkUpdateDialog(loadingDialog);
				else dialogController.Run(loadingDialog);
			}
			catch (ScriptAbortException)
			{
				// do nothing
			}
			catch (InteractiveUserDetachedException)
			{
				// do nothing
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Exception occurred: {e}");
				ShowExceptionDialog(e);
			}
			finally
			{
				Dispose();
			}
		}

		private void ShowNonLiveUserTasksBulkUpdateDialog(NonLiveUserTasksBulkUpdateLoadingDialog loadingDialog)
		{
			var nonLiveBulkUpdateDialog = loadingDialog.NonLiveUserTasksBulkUpdateDialog;
			nonLiveBulkUpdateDialog.SaveCompleted += Dialog_SaveCompleted;

			dialogController.Run(nonLiveBulkUpdateDialog);
		}

		private void Dialog_SaveCompleted(object sender, EventArgs e)
		{
			var messageDialog = new MessageDialog(helpers.Engine, $"Non-Live user tasks successfully updated");
			messageDialog.OkButton.Pressed += (o, ee) => helpers.Engine.ExitSuccess("exit");

			dialogController.ShowDialog(messageDialog);
		}

		private void Initialize(IEngine engine)
		{
			//engine.ShowUI()

			helpers = new Helpers(engine, Scripts.NonLiveUserTasksBulkUpdate);

			dialogController = new InteractiveController(engine);
		}

		private void ShowExceptionDialog(Exception exception)
		{
			var exceptionDialog = new Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog((Engine)helpers.Engine, exception);
			exceptionDialog.OkButton.Pressed += (sender, args) => helpers.Engine.ExitSuccess("Something went wrong.");

			if (dialogController.IsRunning) dialogController.ShowDialog(exceptionDialog);
			else dialogController.Run(exceptionDialog);
		}

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue && disposing)
			{
				helpers?.Dispose();
			}
			
			disposedValue = true;
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