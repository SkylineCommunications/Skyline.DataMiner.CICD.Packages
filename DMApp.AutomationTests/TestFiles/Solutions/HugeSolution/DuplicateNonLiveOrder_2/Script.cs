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

03/02/2020	1.0.0.1		JVT, Skyline	Initial version
****************************************************************************
*/

namespace DuplicateNonLiveOrder_2
{
	using System;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;

	internal class Script : IDisposable
	{
		/*
	This script is a copy of AddOrUpdateNonLiveOrder with minimal changes to allow for duplication
	*/

		private InteractiveController app;

		private LoadNonLiveOrderFormDialog loadNonLiveOrderFormDialog;
		private MainDialog nonLiveOrderForm;
		private NonLiveRejectionDialog rejectionDialog;
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

					loadNonLiveOrderFormDialog = new LoadNonLiveOrderFormDialog(helpers, LoadNonLiveOrderFormDialog.ScriptAction.Duplicate);

					if (loadNonLiveOrderFormDialog.Execute()) ShowNonLiveOrderForm();
					else app.Run(loadNonLiveOrderFormDialog);
				}
				catch (ScriptAbortException)
				{
					throw;
				}
				catch (Exception e)
				{
					if (timeOutResult != -1)
					{
						engine.Log("Run|Something went wrong: " + e);
						ShowExceptionDialog(engine, e);
					}
				}
			});

			timeOutResult = Task.WaitAny(new[] { scriptTask }, new TimeSpan(9, 59, 40));
		}

		private void Initialize(Engine engine)
		{
			engine.Timeout = TimeSpan.FromHours(10);
			//engine.ShowUI();

			helpers = new Helpers(engine, Scripts.DuplicateNonLiveOrder);
			app = new InteractiveController(engine);
		}

		private void ShowNonLiveOrderForm()
		{
			nonLiveOrderForm = loadNonLiveOrderFormDialog.NonLiveOrderFormDialog;
			nonLiveOrderForm.BookOrSaveFinished += NonLiveOrderForm_BookOrSaveFinished;
			nonLiveOrderForm.RejectOrderButton.Pressed += Dialog_RejectOrderButton_Pressed;

			if(app.IsRunning) app.ShowDialog(nonLiveOrderForm);
			else app.Run(nonLiveOrderForm);
		}

		private void NonLiveOrderForm_BookOrSaveFinished(object sender, Library_1.EventArguments.StringEventArgs e)
		{
			ShowMessageDialog(e.Value, "Duplicate Non-Live Order finished");
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			ExceptionDialog exceptionDialog = new ExceptionDialog(engine, exception);
			exceptionDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong during the creation of the new Order.");

			if (app.IsRunning) app.ShowDialog(exceptionDialog);
			else app.Run(exceptionDialog);
		}

		private void ShowMessageDialog(string message, string title)
		{
			MessageDialog dialog = new MessageDialog(helpers.Engine, message) { Title = title };
			dialog.OkButton.Pressed += (sender, args) => helpers.Engine.ExitSuccess(message);

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}

		private void Dialog_RejectOrderButton_Pressed(object sender, EventArgs e)
		{
			rejectionDialog = new NonLiveRejectionDialog(helpers.Engine);
			rejectionDialog.CancelButton.Pressed += RejectionDialog_CancelButton_Pressed;
			rejectionDialog.RejectButton.Pressed += RejectionDialog_RejectButton_Pressed;

			app.ShowDialog(rejectionDialog);
		}

		private void RejectionDialog_RejectButton_Pressed(object sender, EventArgs e)
		{
			if (!rejectionDialog.IsValid())
			{
				return;
			}

			rejectionDialog.UpdateNonLiveOrder(nonLiveOrderForm.NonLiveOrder);

			helpers.NonLiveOrderManager.SetNonLiveOrderToChangeRequested(nonLiveOrderForm.NonLiveOrder, loadNonLiveOrderFormDialog.UserInfo.User);

			ShowMessageDialog("Order successfully rejected", "Reject Non Live Order");
		}

		private void RejectionDialog_CancelButton_Pressed(object sender, EventArgs e)
		{
			app.ShowDialog(nonLiveOrderForm);
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



