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

31/07/2020	1.0.0.1		TIMGE, Skyline	Initial version
****************************************************************************
*/

using ShowFeenixDetails_2.Feenix;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

//---------------------------------
// ShowFeenixDetails_2.cs
//---------------------------------

namespace Feenix
{
	using System;
	using ShowFeenixDetails_2.Dialogs;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;

	/// <summary>
	/// DataMiner Script Class.
	/// Engine.ShowUI();
	/// </summary>
	public class Script : IDisposable
	{
		private Helpers helpers;
		private InteractiveController app;

		private DetailsDialog detailsDialog;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			engine.SetFlag(RunTimeFlags.NoKeyCaching);
			engine.SetFlag(RunTimeFlags.NoInformationEvents);
			engine.Timeout = TimeSpan.FromHours(10);

			helpers = new Helpers(engine, Scripts.ShowFeenixDetails);

			app = new InteractiveController(engine);

			try
			{
				string liveStreamOrderId = engine.GetScriptParam("ID").Value;
				if (!Guid.TryParse(liveStreamOrderId, out Guid orderGuid))
				{
					string message = "Error while parsing the order ID";
					helpers.Log(nameof(Script), nameof(Run), message);
					var errorDialog = new MessageDialog(engine, message) { Title = "Show order details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Feenix details are not available for this order");
					app.Run(errorDialog);
					return;
				}

				LiteOrder order = helpers.OrderManager.GetLiteOrder(orderGuid, true);
				if (order == null)
				{
					string message = "Unable to get order";
					helpers.Log(nameof(Script), nameof(Run), message);
					var errorDialog = new MessageDialog(engine, message) { Title = "Show order details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Feenix details are not available for this order");
					app.Run(errorDialog);
					return;
				}

				string areenaId = order.YleId;
				if (String.IsNullOrWhiteSpace(areenaId))
				{
					string message = "Feenix details are not available for this order";
					helpers.Log(nameof(Script), nameof(Run), message);
					var errorDialog = new MessageDialog(engine, message) { Title = "Show order details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Feenix details are not available for this order");
					app.Run(errorDialog);
					return;
				}

				FeenixManager feenixManager = new FeenixManager(helpers);
				LiveStreamOrder liveStreamOrder = feenixManager.GetLiveStreamOrder(areenaId);
				if (liveStreamOrder == null)
				{
					string message = $"Unable to get order with {areenaId} from the Feenix element";
					helpers.Log(nameof(Script), nameof(Run), message);
					var errorDialog = new MessageDialog(engine, message) { Title = "Show order details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Feenix details are not available for this order");
					app.Run(errorDialog);
					return;
				}

				detailsDialog = new DetailsDialog(engine, liveStreamOrder);
				detailsDialog.OkButton.Pressed += (sender, args) => { engine.ExitSuccess("OK"); };

				app.Run(detailsDialog);
			}
			catch (FeenixElementException f)
			{
				string message = "Feenix element can not be found or is inactive";
				helpers.Log(nameof(Script), nameof(Run), message);
				var errorDialog = new MessageDialog(engine, message) { Title = "Unable to show order details" };
				errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Feenix element can not be found or is inactive: " + f.Message);
				app.Run(errorDialog);
			}
			catch (ScriptAbortException)
			{
				// nothing to do
			}
			catch (InteractiveUserDetachedException)
			{
				// nothing to do
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Exception occurred: {e}");
				ShowExceptionDialog(engine, e);
			}
			finally
			{
				Dispose();
			}
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			ExceptionDialog dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong while executing Show Feenix Details script.");

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

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