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

namespace CancelMultipleOrders_2
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using OrderStatus = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	class Script : IDisposable
	{
		private Helpers helpers;
		private InteractiveController app;
		private ProvideReasonForStatusChangeDialog provideReasonForStatusChangeDialog;
		private CancelMultipleOrdersProgressDialog cancelMultipleOrdersProgresssDialog;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			try
			{
				//engine.ShowUI();
				engine.Timeout = TimeSpan.FromHours(10);

				app = new InteractiveController(engine);
				helpers = new Helpers(engine, Scripts.CancelMultipleOrders);

				provideReasonForStatusChangeDialog = new ProvideReasonForStatusChangeDialog(engine, OrderStatus.Cancelled);
				provideReasonForStatusChangeDialog.OkButton.Pressed += CancelOrders;
				app.Run(provideReasonForStatusChangeDialog);
			}
			catch (ScriptAbortException)
			{
				// nothing
			}
			catch (InteractiveUserDetachedException)
			{
				// nothing
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Something went wrong: {e}");
				ShowExceptionDialog(e);
			}
			finally
			{
				helpers.LockManager.ReleaseLocks();
				Dispose();
			}
		}

		private void CancelOrders(object sender, EventArgs e)
		{
			cancelMultipleOrdersProgresssDialog = new CancelMultipleOrdersProgressDialog(helpers, provideReasonForStatusChangeDialog.ReasonForStatusChange);
			cancelMultipleOrdersProgresssDialog.Execute();
			helpers.LockManager.ReleaseLocks();

			if (app.IsRunning) app.ShowDialog(cancelMultipleOrdersProgresssDialog);
			else app.Run(cancelMultipleOrdersProgresssDialog);
		}

		private void ShowExceptionDialog(Exception exception)
		{
			var dialog = new ExceptionDialog((Engine)helpers.Engine, exception);
			dialog.OkButton.Pressed += (sender, args) => helpers.Engine.ExitSuccess("Something went wrong during the creation of the new Order.");

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}

		#region IDisposable Support
		private bool disposedValue; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue && disposing)
			{
				helpers.Dispose();
			}

			disposedValue = true;	
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