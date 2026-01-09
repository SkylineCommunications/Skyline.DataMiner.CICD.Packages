using System;
using ShowPebbleBeachDetails_2.PebbleBeach;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

namespace ShowPebbleBeachDetails_2
{
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
			// Do not remove
			// Engine.ShowUI();

			app = new InteractiveController(engine);
			helpers = new Helpers(engine, Scripts.ShowPebbleBeachDetails);

			try
			{
				Dialog detailsDialog = null;

				PebbleBeachManager pebbleBeachManager = new PebbleBeachManager(engine);
				OrderManager orderManager = new OrderManager(helpers);

				string paramGuid = engine.GetScriptParam("GUID").Value;

				if (!Guid.TryParse(paramGuid, out Guid checkedGuid))
				{
					var errorDialog = new MessageDialog(engine, "Error while parsing the GUID") { Title = "Show Pebble Beach details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Pebble Beach details are not available for this order");
					app.Run(errorDialog);
					return;
				}

				LiteOrder order = orderManager.GetLiteOrder(checkedGuid, true);
				if (order == null)
				{
					var errorDialog = new MessageDialog(engine, "Pebble Beach details are not available for this order") { Title = "Show Pebble Beach details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Pebble Beach details are not available for this order");
					app.Run(errorDialog);
					return;
				}

				string plasmaId = order.PlasmaId;
				if (String.IsNullOrWhiteSpace(plasmaId))
				{
					var errorDialog = new MessageDialog(engine, "Pebble Beach details are not available for this order") { Title = "Show Pebble Beach details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Pebble Beach details are not available for this order");
					app.Run(errorDialog);
					return;
				}

				PebbleBeachEvent pebbleBeachEvent = pebbleBeachManager.GetEvent(plasmaId);

				if (pebbleBeachEvent != null)
				{
					detailsDialog = new PebbleBeachEventDetails(engine, pebbleBeachEvent);
					app.Run(detailsDialog);
				}
				else
				{
					var errorDialog = new MessageDialog(engine, "Pebble Beach details not found") { Title = "Show Pebble Beach details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Pebble Beach details are not available for this Event");
					app.Run(errorDialog);
				}
			}
			catch (Exception e)
			{
				engine.Log("Run|Something went wrong: " + e.Message + e.StackTrace);
				ShowExceptionDialog(engine, e);
			}
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			ExceptionDialog dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitFail("Something went wrong.");
			if (app.IsRunning) app.ShowDialog(dialog); else app.Run(dialog);
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