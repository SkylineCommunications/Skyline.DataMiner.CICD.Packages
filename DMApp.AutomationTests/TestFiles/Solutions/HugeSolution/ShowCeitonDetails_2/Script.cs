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

dd/mm/2020	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

// Engine.ShowUI();

namespace ShowCeitonDetails_2
{
	using System;
	using ShowCeitonDetails_2.Ceiton;
	using ShowCeitonDetails_2.Dialogs;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
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

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			engine.Timeout = TimeSpan.FromHours(10);
			engine.SetFlag(RunTimeFlags.NoKeyCaching);

			helpers = new Helpers(engine, Scripts.ShowCeitonDetails);
			app = new InteractiveController(engine);

			try
			{
				string eventId = engine.GetScriptParam("Event ID").Value;

				if (!Guid.TryParse(eventId, out var eventGuid))
				{
					var errorDialog = new MessageDialog(engine, "Error while parsing the Event ID") { Title = "Show event details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Ceiton details are not available for this Event");
					app.Run(errorDialog);
					return;
				}

				var @event = helpers.EventManager.GetEvent(eventGuid);
				if (@event == null)
				{
					var errorDialog = new MessageDialog(engine, "Unable to get Event") { Title = "Show event details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Ceiton details are not available for this Event");
					app.Run(errorDialog);
					return;
				}

				string projectNumber = @event.ProjectNumber;

				if (string.IsNullOrWhiteSpace(projectNumber))
				{
					var errorDialog = new MessageDialog(engine, "Ceiton details are not available for this Event") { Title = "Show event details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Ceiton details are not available for this Event");
					app.Run(errorDialog);
					return;
				}

				var ceitonManager = new CeitonManager(helpers);
				if (!ceitonManager.ProjectExists(projectNumber))
				{
					var errorDialog = new MessageDialog(engine, "Ceiton details are not available for this Event") { Title = "Show event details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Ceiton details are not available for this Event");
					app.Run(errorDialog);
					return;
				}

				var detailsDialog = new DetailsDialog(engine, ceitonManager, projectNumber);

				app.Run(detailsDialog);
			}
			catch (ScriptAbortException)
			{
				// do nothing
			}
			catch (CeitonElementException)
			{
				var errorDialog = new MessageDialog(engine, "Ceiton element can not be found or is inactive") { Title = "Unable to show event details" };
				errorDialog.OkButton.Pressed += (sender, args) => engine.ExitFail("Ceiton element can not be found or is inactive");
				app.Run(errorDialog);
			}
			catch (Exception e)
			{
				engine.Log("Run|Something went wrong: " + e);
				ShowExceptionDialog(engine, e);
			}
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			var dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong while executing the Show Ceiton Details.");

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
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
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}