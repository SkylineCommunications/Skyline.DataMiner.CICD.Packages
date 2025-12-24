namespace ExecuteFeenixStopCommand_2
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Feenix;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations.Feenix;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	internal class Script : IDisposable
	{
		// Do not remove this
		// Engine.ShowUI();

		private InteractiveController app;
		private Helpers helpers;

		private bool disposedValue;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			engine.Timeout = TimeSpan.FromHours(10);

			app = new InteractiveController(engine);
			helpers = new Helpers(engine, Scripts.ExecuteFeenixStopCommand);

			try
			{
				string liveStreamOrderId = engine.GetScriptParam("ID").Value;
				if (!Guid.TryParse(liveStreamOrderId, out Guid orderGuid))
				{
					var errorDialog = new MessageDialog(engine, "Error while parsing the order ID") { Title = "Show order details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Error while parsing the order ID");
					app.Run(errorDialog);
					return;
				}

				OrderManager orderManager = new OrderManager(helpers);
				LiteOrder order = orderManager.GetLiteOrder(orderGuid, true);
				if (order == null)
				{
					var errorDialog = new MessageDialog(engine, "Unable to get the order") { Title = "Show order details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Unable to get the order");
					app.Run(errorDialog);
					return;
				}

				string areenaId = order.YleId;
				if (String.IsNullOrWhiteSpace(areenaId))
				{
					var errorDialog = new MessageDialog(engine, "Areena ID couldn't be found inside this order") { Title = "Show order details" };
					errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Areena ID couldn't be found inside this order");
					app.Run(errorDialog);
					return;
				}
				else
				{
					OrderManagerElement orderManagerElement = new OrderManagerElement(helpers);
					FeenixElement feenixManager = new FeenixElement(orderManagerElement.FeenixElement);

					ConfirmDialog confirmDialog = new ConfirmDialog(engine);
					confirmDialog.NoButton.Pressed += (sender, args) => { engine.ExitSuccess(order.Id.ToString()); };
					confirmDialog.YesButton.Pressed += (sender, args) =>
					{
						feenixManager.SendStopNotification(areenaId);
						engine.ExitSuccess(order.Id.ToString());
					};

					app.Run(confirmDialog);
				}
			}
			catch (FeenixElementException f)
			{
				var errorDialog = new MessageDialog(engine, "Feenix element can not be found or is inactive") { Title = "Unable to show order details" };
				errorDialog.OkButton.Pressed += (sender, args) => engine.ExitFail("Feenix element can not be found or is inactive: " + f.Message);
				app.Run(errorDialog);
			}
			catch (ScriptAbortException)
			{
				// Do nothing.
			}
			catch (Exception e)
			{
				engine.Log("Run|Something went wrong: " + e);
				ShowExceptionDialog(engine, e);
			}
		}

		private void ShowExceptionDialog(Engine engine, Exception exception)
		{
			ExceptionDialog dialog = new ExceptionDialog(engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitFail("Something went wrong.");

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