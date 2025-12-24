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

30/07/2020	1.0.0.1		TIMGE, Skyline	Initial version
04/09/2020	1.0.0.2		TRE, Skyline	Program details are now also available for a Plasma Order
14/10/2020	1.0.0.3		GVH, Skyline	Small adaptations for the plasma details on a plasma order, also plasma details aren't shown on event level
16/10/2020	1.0.0.4		TRE, Skyline	Sped up retrieval of transmission info from the Plasma Element.
02/03/2021	1.0.0.5		TRE, Skyline	Added additional Finnish and Swedish general program fields.
****************************************************************************
*/

//Engine.ShowUI();

namespace ShowPlasmaDetails_2
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.Exceptions;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.ParsedObjects;
	using Skyline.DataMiner.Utils.YLE.Integrations.Plasma;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private Helpers helpers;
		private InteractiveController app;
		private PlasmaElement plasmaElement;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			try
			{
				Initialize(engine);

				RunSafe();
			}
			catch (ScriptAbortException)
			{
				throw;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Exception: {e}");
				ShowExceptionDialog(engine, e);
			}
			finally
			{
				Dispose();
			}
		}

		private void Initialize(Engine engine)
		{
			engine.Timeout = TimeSpan.FromHours(10);
			//engine.ShowUI();

			helpers = new Helpers(engine, Scripts.ShowPlasmaDetails);
		}

		private void RunSafe()
		{
			plasmaElement = new PlasmaElement(helpers.OrderManagerElement.PlasmaElement);
			app = new InteractiveController(helpers.Engine);

			var paramGuid = helpers.Engine.GetScriptParam("GUID").Value;

			if (!Guid.TryParse(paramGuid, out var checkedGuid))
			{
				var errorDialog = new MessageDialog(helpers.Engine, "Error while parsing the Order ID") {Title = "Show Plasma details"};
				errorDialog.OkButton.Pressed += (sender, args) => helpers.Engine.ExitSuccess("Plasma details are not available for this Order");
				app.Run(errorDialog);
				return;
			}

			var order = helpers.OrderManager.GetLiteOrder(checkedGuid, true);

			if (order == null)
			{
				var errorDialog = new MessageDialog(helpers.Engine, "Plasma details are not available") {Title = "Show Plasma details"};
				errorDialog.OkButton.Pressed += (sender, args) => helpers.Engine.ExitSuccess("For the given ID, no orders or events were available.");
				app.Run(errorDialog);
				return;
			}

			if (string.IsNullOrWhiteSpace(order.PlasmaId))
			{
				var errorDialog = new MessageDialog(helpers.Engine, "Plasma ID not available") {Title = "Show Plasma details"};
				errorDialog.OkButton.Pressed += (sender, args) => helpers.Engine.ExitSuccess("No Plasma ID available for this order.");
				app.Run(errorDialog);
				return;
			}

			try
			{

				ParsedPlasmaOrder plasmaOrder;

				if (!plasmaElement.TryGetPlasmaOrder(order.EditorialObjectId, out plasmaOrder))
				{
					plasmaElement.TryGetPlasmaOrderByPlasmaId(order.PlasmaId, out plasmaOrder);
				}

				if (plasmaOrder == null)
				{
					var errorDialog = new MessageDialog(helpers.Engine, "Unable to get data from Plasma element") { Title = "Show Plasma details" };
					errorDialog.OkButton.Pressed += (sender, args) => helpers.Engine.ExitSuccess("Unable to get data from Plasma element.");
					app.Run(errorDialog);
					return;
				}

				var detailsDialog = new TransmissionDetailsDialog((Engine)helpers.Engine, plasmaOrder);
				app.Run(detailsDialog);
			}
			catch (PublicationEventNotFoundException e)
			{
				var errorDialog = new MessageDialog(helpers.Engine, e.Message) { Title = "Show Plasma details" };
				errorDialog.OkButton.Pressed += (sender, args) => helpers.Engine.ExitSuccess(e.Message);
				app.Run(errorDialog);
				return;
			}
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			var dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong while executing the Show Plasma Details script.");

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}

		#region IDisposable Support

		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing) helpers.Dispose();

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