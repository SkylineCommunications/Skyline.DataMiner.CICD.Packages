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

18/11/2021	1.0.0.1		TRE, Skyline	Initial version
****************************************************************************
*/



//---------------------------------
// UpdateVisibilityRights_1.cs
//---------------------------------

namespace UpdateVisibilityRights_1
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	public class Script
	{
		private Engine engine;
		private InteractiveController app;
		private UpdateRightsDialog updateRightsDialog;
		private ProgressDialog progressDialog;
		private OrderHelper orderHelper;

		/// <summary>
		/// The Script entry point.
		/// Engine.ShowUI();
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			try
			{
				this.engine = engine;
				engine.SetFlag(RunTimeFlags.NoKeyCaching);
				engine.Timeout = TimeSpan.FromHours(10);
				RunSafe();
			}
			catch (ScriptAbortException)
			{
				throw;
			}
			catch (Exception e)
			{
				engine.Log("Run|Something went wrong: " + e);
				ShowExceptionDialog(engine, e);
			}
		}

		private void RunSafe()
		{
			var helpers = new Helpers(engine, Scripts.UpdateVisibilityRights);
			app = new InteractiveController(engine);
			orderHelper = new OrderHelper(helpers);
			ContractManager contractManager = new ContractManager(engine);

			engine.Log("showing dialog");

			updateRightsDialog = new UpdateRightsDialog(engine, contractManager.ViewIds);
			updateRightsDialog.FindOrderByGuidButton.Pressed += FindOrderByGuidButton_Pressed;
			updateRightsDialog.FindOrderByNameButton.Pressed += FindOrderByNameButton_Pressed;
			updateRightsDialog.UpdateSecurityViewIdsButton.Pressed += UpdateSecurityViewIds_Pressed;
			app.Run(updateRightsDialog);
		}

		private void FindOrderByNameButton_Pressed(object sender, EventArgs e)
		{
			if (String.IsNullOrWhiteSpace(updateRightsDialog.OrderNameTextBox.Text))
			{
				updateRightsDialog.OrderGuidTextBox.Text = String.Empty;
				updateRightsDialog.Order = null;
				updateRightsDialog.OrderNameTextBox.ValidationText = "Fill out the Name of an order";
				updateRightsDialog.OrderNameTextBox.ValidationState = UIValidationState.Invalid;
				updateRightsDialog.GenerateUI();
				return;
			}

			BookedOrder order = orderHelper.GetOrder(updateRightsDialog.OrderNameTextBox.Text);
			if (order == null)
			{
				updateRightsDialog.OrderGuidTextBox.Text = String.Empty;
				updateRightsDialog.Order = null;
				updateRightsDialog.OrderNameTextBox.ValidationText = "Unable to find an order with the specified Name";
				updateRightsDialog.OrderNameTextBox.ValidationState = UIValidationState.Invalid;
				updateRightsDialog.GenerateUI();
				return;
			}

			updateRightsDialog.OrderNameTextBox.Text = order.Reservation.Name;
			updateRightsDialog.OrderGuidTextBox.Text = order.Reservation.ID.ToString();
			updateRightsDialog.OrderNameTextBox.ValidationState = UIValidationState.Valid;

			updateRightsDialog.Order = order;
		}

		private void FindOrderByGuidButton_Pressed(object sender, EventArgs e)
		{
			if(!Guid.TryParse(updateRightsDialog.OrderGuidTextBox.Text, out Guid guid))
			{
				updateRightsDialog.OrderNameTextBox.Text = String.Empty;
				updateRightsDialog.Order = null;
				updateRightsDialog.OrderNameTextBox.ValidationText = "Unable to parse the GUID";
				updateRightsDialog.OrderGuidTextBox.ValidationState = UIValidationState.Invalid;
				updateRightsDialog.GenerateUI();
				return;
			}

			BookedOrder order = orderHelper.GetOrder(guid);
			if (order == null)
			{
				updateRightsDialog.OrderNameTextBox.Text = String.Empty;
				updateRightsDialog.Order = null;
				updateRightsDialog.OrderNameTextBox.ValidationText = "No orders found with the given ID";
				updateRightsDialog.OrderGuidTextBox.ValidationState = UIValidationState.Invalid;
				updateRightsDialog.GenerateUI();
				return;
			}

			updateRightsDialog.OrderNameTextBox.Text = order.Reservation.Name;
			updateRightsDialog.OrderGuidTextBox.Text = order.Reservation.ID.ToString();
			updateRightsDialog.OrderGuidTextBox.ValidationState = UIValidationState.Valid;

			updateRightsDialog.Order = order;
		}

		private void UpdateSecurityViewIds_Pressed(object sender, EventArgs e)
		{
			progressDialog = new ProgressDialog(engine);
			progressDialog.OkButton.Pressed += (s, args) => app.ShowDialog(updateRightsDialog);
			app.ShowDialog(progressDialog);

			try
			{
				orderHelper.UpdateVisibilityRights(
					updateRightsDialog.Order,
					updateRightsDialog.SelectedOrderSecurityViewIds,
					updateRightsDialog.SelectedEventSecurityViewIds,
					progressDialog
				);
			}
			catch (Exception exception)
			{
				progressDialog.AddProgressLine("Something went wrong: " + exception);
				engine.Log("UpdateSecurityViewIds_Pressed|" + exception);
			}

			progressDialog.Finish();
			app.ShowDialog(progressDialog);
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			ExceptionDialog dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitFail("Something went wrong during the creation of the new event.");
			if (app.IsRunning) app.ShowDialog(dialog); else app.Run(dialog);
		}
	}
}
//---------------------------------
// BookedOrder.cs
//---------------------------------

//---------------------------------
// BookedService.cs
//---------------------------------

//---------------------------------
// ContractManager.cs
//---------------------------------

//---------------------------------
// OrderHelper.cs
//---------------------------------

//---------------------------------
// UpdateRightsDialog.cs
//---------------------------------