namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.ServiceConfigurations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using System.IO;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Configuration;

	public class FixServiceConfigurationsDialog : DebugDialog
	{
		private readonly Label fixServiceConfigurationsLabel = new Label("Fix Service Configurations") { Style = TextStyle.Heading };

		private GetOrderReservationsSection getOrderReservationsSection;

		private readonly Button fixServiceConfigurationsButton = new Button("Fix Service Configurations") { Width = 200 };

		private readonly Label descriptionLabel = new Label("This feature retrieves the last saved service configuration from the order logging and applies it to the order.");

		public FixServiceConfigurationsDialog(Helpers helpers) : base(helpers)
		{
			Title = "Fix Service Configurations";

			Initialize();
			GenerateUi();
		}

		public event EventHandler<ConfirmationDialog> ShowConfirmationDialog;

		public event EventHandler<ProgressDialog> ShowProgressDialog;

		public event EventHandler OrdersUpdated;

		private void Initialize()
		{
			getOrderReservationsSection = new GetOrderReservationsSection(helpers);

			fixServiceConfigurationsButton.Pressed += (s, e) => FixServiceConfigurationsButton_Pressed();
		}

		private void FixServiceConfigurationsButton_Pressed()
		{
			if (!getOrderReservationsSection.IsValid)
			{
				ShowRequestResult("Invalid order IDs", "Invalid Order IDs");
			}

			var orderReservations = getOrderReservationsSection.GetOrderReservations().OrderBy(x => x.Start).ToList();
			if (!orderReservations.Any()) return;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"Are you sure you want to fix the service configurations for following {orderReservations.Count} Order(s):");
			foreach (var orderReservation in orderReservations) sb.AppendLine($"\t-{orderReservation.Name} ({orderReservation.Start.ToLocalTime()} - {orderReservation.End.ToLocalTime()})");

			ConfirmationDialog confirmationDialog = new ConfirmationDialog(helpers.Engine, sb.ToString());

			confirmationDialog.YesButton.Pressed += (s, args) => FixServiceConfigurations(orderReservations);

			ShowConfirmationDialog?.Invoke(this, confirmationDialog);
		}

		private void FixServiceConfigurations(IEnumerable<ServiceReservationInstance> orderReservations)
		{
			ProgressDialog progressDialog = new ProgressDialog(helpers.Engine) { Title = "Fixing Service Configurations" };
			progressDialog.OkButton.Pressed += (sender, args) => OrdersUpdated?.Invoke(this, EventArgs.Empty);
			progressDialog.Show(false);

			List<ServiceReservationInstance> validOrders = new List<ServiceReservationInstance>();
			List<ServiceReservationInstance> ordersToFix = new List<ServiceReservationInstance>();

			progressDialog.AddProgressLine($"Analyzing orders...");
			foreach (var orderReservation in orderReservations)
			{
				try
				{
					helpers.OrderManager.GetOrder(orderReservation);
					validOrders.Add(orderReservation);

					progressDialog.AddProgressLine($"Order {orderReservation.Name} is valid, no need to fix service configuration");
				}
				catch (Exception)
				{
					ordersToFix.Add(orderReservation);
				}
			}

			progressDialog.AddProgressLine($"Finished analyzing orders");

			List<ServiceReservationInstance> fixedOrders = new List<ServiceReservationInstance>();
			Dictionary<ServiceReservationInstance, Exception> unfixedOrders = new Dictionary<ServiceReservationInstance, Exception>();

			progressDialog.AddProgressLine($"Fixing orders...");
			foreach (var order in ordersToFix)
			{
				try
				{
					string fileName = $@"{FixedFileLogger.SkylineDataFilePath}{Configuration.Constants.OrderLoggingDirectoryName}\{order.ID}{FixedFileLogger.TextFileExtension}";

					string lineContainingLastSave = File.ReadAllLines(fileName).Last(x => x.Contains("OrderManagerElement|AddOrUpdateServiceConfigurations|Saving service configurations:"));

					int startIndex = lineContainingLastSave.IndexOf('{');
					int endIndex = lineContainingLastSave.LastIndexOf('}');
					string serializedServiceConfiguration = lineContainingLastSave.Substring(startIndex, endIndex - startIndex + 1);

					JsonConvert.DeserializeObject<Dictionary<int, ServiceConfiguration>>(serializedServiceConfiguration);

					if (!helpers.OrderManagerElement.AddOrUpdateServiceConfigurations(order.ID, order.End, serializedServiceConfiguration)) throw new InvalidOperationException("Unable to save service configuration");

					fixedOrders.Add(order);

					progressDialog.AddProgressLine($"Fixing order {order.Name}");
				}
				catch (Exception e)
				{
					unfixedOrders.Add(order, e);

					progressDialog.AddProgressLine($"Unable to fix order {order.Name}");
				}
			}

			progressDialog.AddProgressLine($"Finished fixing orders");

			string validOrdersResult = $"Valid Orders (no service config fix required):\n{String.Join("\n", validOrders.Select(x => $"{x.Name} [{x.Start} - {x.End}]"))}";
			string fixedOrdersResult = $"Fixed Orders:\n{String.Join("\n", fixedOrders.Select(x => $"{x.Name} [{x.Start} - {x.End}]"))}";
			string unfixedOrdersResult = $"Unable to fix following orders:\n{String.Join("\n", unfixedOrders.Select(x => $"{x.Key.Name} [{x.Key.Start} - {x.Key.End}] due to: {x.Value}"))}";

			ShowRequestResult($"Updated Orders {DateTime.Now.ToShortTimeString()}", new string[] { fixedOrdersResult, unfixedOrdersResult, validOrdersResult });
			GenerateUi();

			progressDialog.Finish();
			ShowProgressDialog?.Invoke(this, progressDialog);
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(fixServiceConfigurationsLabel, ++row, 0, 1, 5);

			AddWidget(descriptionLabel, ++row, 0, 1, 5);

			AddSection(getOrderReservationsSection, ++row, 0);
			row += getOrderReservationsSection.RowCount;

			AddWidget(fixServiceConfigurationsButton, ++row, 0, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 1);

			AddResponseSections(row);
		}
	}
}
