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

namespace ShowEBUDetails_2
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations.Eurovision;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private InteractiveController app;
		private Helpers helpers;
		private bool disposedValue;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			try
			{
				//engine.ShowUI();

				engine.SetFlag(RunTimeFlags.NoKeyCaching);
				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				// Do nothing
			}
			catch (InteractiveUserDetachedException)
			{
				// Do nothing
			}
			catch (Exception e)
			{
				engine.Log("Run|Something went wrong: " + e);
				ShowExceptionDialog(engine, e);
			}
		}

		private void RunSafe(Engine engine)
		{
			app = new InteractiveController(engine);
			helpers = new Helpers(engine, Scripts.ShowEBUDetails);

			var id = engine.GetScriptParam("ID").Value;
			if (!Guid.TryParse(id, out var orderId))
			{
				var errorDialog = new MessageDialog(engine, String.Format("Invalid order id: {0}", id)) { Title = "Invalid order id" };
				errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess(String.Format("Invalid order id: {0}", id));
				app.Run(errorDialog);
				return;
			}

			// Retrieve Order
			OrderManager orderManager = new OrderManager(helpers);
			Order order = orderManager.GetOrder(orderId);

			if (order == null)
			{
				var errorDialog = new MessageDialog(engine, String.Format("Unable to retrieve Order with ID: {0}", orderId)) { Title = "Unable to retrieve Order" };
				errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess(String.Format("Unable to retrieve Order with ID: {0}", orderId));
				app.Run(errorDialog);
				return;
			}

			// Check if Order has a Eurovision Number
			if (String.IsNullOrWhiteSpace(order.EurovisionTransmissionNumber))
			{
				var errorDialog = new MessageDialog(engine, "Not a Eurovision Order") { Title = "Not a Eurovision Order" };
				errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Not a Eurovision Order");
				app.Run(errorDialog);
				return;
			}

			// Retrieve Synopsis
			string synopsis = null;
			try
			{
				var ebuElements = engine.FindElementsByProtocol("EBU Synopsis Web Service");
				OrderManagerElement orderManagerElement = new OrderManagerElement(helpers);
				var ebuManager = new EurovisionElement(orderManagerElement.EbuElement);
				synopsis = ebuManager.GetSynopsisText(order.EurovisionTransmissionNumber);
			}
			catch (Exception e)
			{
				engine.Log("Run|Something went wrong with retrieving the Synopsis: " + e);

				var errorDialog = new MessageDialog(engine, "Unable to retrieve the synopsis for the Order: " + e) { Title = "Unable to retrieve synopsis" };
				errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Synopsis is not available for this order");
				app.Run(errorDialog);
				return;
			}

			if (synopsis == null)
			{
				var errorDialog = new MessageDialog(engine, String.Format("Unable to retrieve synopsis from Order with ID: {0}", orderId)) { Title = "Unable to retrieve synopsis" };
				errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Synopsis is not available for this order");
				app.Run(errorDialog);
				return;
			}

			EbuDetailsDialog detailsDialog = new EbuDetailsDialog(engine, order.EurovisionTransmissionNumber, synopsis);
			detailsDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess(String.Empty);
			app.Run(detailsDialog);
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			ExceptionDialog dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong during the creation of the new event.");

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