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
****************************************************************************
*/



//---------------------------------
// DeleteReservationsForCertainResource_1.cs
//---------------------------------

namespace DeleteReservationsForCertainResource_1
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script
	{
		private Engine engine;
		private InteractiveController app;

		private DeleteReservationsFromResourceDialog handleResourcesFromReservationsDialog;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			try
			{
				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				throw;
			}
			catch (Exception e)
			{
				ShowExceptionDialog(engine, e);
			}
		}

		private void RunSafe(Engine engine)
		{
			// engine.ShowUI()
			engine.Timeout = TimeSpan.FromHours(10);
			this.engine = engine;

			app = new InteractiveController(engine);

			handleResourcesFromReservationsDialog = new DeleteReservationsFromResourceDialog(engine);
			handleResourcesFromReservationsDialog.DeleteSelectedReservationsButton.Pressed += DeleteSelectedReservationsButton_Pressed;
			app.Run(handleResourcesFromReservationsDialog);
		}

		private void DeleteSelectedReservationsButton_Pressed(object sender, EventArgs e)
		{
			ConfirmDeleteDialog confirmDeleteDialog = new ConfirmDeleteDialog(engine);
			confirmDeleteDialog.NoButton.Pressed += (o, args) => app.ShowDialog(handleResourcesFromReservationsDialog);
			confirmDeleteDialog.YesButton.Pressed += (o, args) => DeleteReservations();

			app.ShowDialog(confirmDeleteDialog);
		}

		private void DeleteReservations()
		{
			handleResourcesFromReservationsDialog.RemoveSelectedReservationInstances();

			ShowMessageDialog(engine, "Removing the selected reservation instance(s) succeeded", "Removing Selected Reservation(s)");
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
			dialog.OkButton.Pressed += (sender, args) => engine.ExitFail("Something went wrong during the handling of reservation deletion");
			if (!app.IsRunning) app.Run(dialog);
			else app.ShowDialog(dialog);
		}
	}
}
//---------------------------------
// ConfirmDeleteDialog.cs
//---------------------------------

//---------------------------------
// DeleteReservationsFromResourceDialog.cs
//---------------------------------