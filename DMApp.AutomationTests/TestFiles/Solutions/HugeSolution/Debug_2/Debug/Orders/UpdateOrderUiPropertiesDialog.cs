namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Orders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class UpdateOrderUiPropertiesDialog : Dialog
	{
		private readonly Helpers helpers;

		private readonly Label updateUiPropertiesLabel = new Label("Update UI Properties") { Style = TextStyle.Heading };

		private GetOrderReservationsSection getOrderReservationsSection;

		private readonly Button updateUiPropertiesButton = new Button("Update UI Properties") { Width = 200 };

		private readonly CheckBox updateRecordingServicesCheckBox = new CheckBox("Update UI Properties of Services") { IsChecked = false };

		private readonly List<RequestResultSection> responseSections = new List<RequestResultSection>();

		public UpdateOrderUiPropertiesDialog(Helpers helpers) : base(helpers.Engine)
		{
			Title = "Update Order UI Properties";
			this.helpers = helpers;

			Initialize();
			GenerateUi();
		}

		public Button BackButton { get; } = new Button("Back...") { Width = 150 };

		public event EventHandler<ConfirmationDialog> ShowConfirmationDialog;

		public event EventHandler<ProgressDialog> ShowProgressDialog;

		public event EventHandler OrdersUpdated;

		private void Initialize()
		{
			getOrderReservationsSection = new GetOrderReservationsSection(helpers);

			updateUiPropertiesButton.Pressed += UpdateUiPropertiesButton_Pressed;
		}

		private void UpdateUiPropertiesButton_Pressed(object sender, EventArgs e)
		{
			if (!getOrderReservationsSection.IsValid)
			{
				ShowRequestResult("Invalid order IDs", "Invalid Order IDs");
			}

			var orderReservations = getOrderReservationsSection.GetOrderReservations();
			if (!orderReservations.Any()) return;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"Are you sure you want to update the UI properties on the following {orderReservations.Count} order(s):");
			foreach (var orderReservation in orderReservations) sb.AppendLine($"\t-{orderReservation.Name}[{orderReservation.ID}]");

			ConfirmationDialog confirmationDialog = new ConfirmationDialog(helpers.Engine, sb.ToString());

			confirmationDialog.YesButton.Pressed += (s, args) => UpdateUiProperties(orderReservations, updateRecordingServicesCheckBox.IsChecked);

			ShowConfirmationDialog?.Invoke(this, confirmationDialog);
		}

		private void UpdateUiProperties(IEnumerable<ServiceReservationInstance> orderReservations, bool updateServices)
		{
			ProgressDialog progressDialog = new ProgressDialog(helpers.Engine) { Title = "Updating UI Properties" };
			progressDialog.OkButton.Pressed += (sender, args) => OrdersUpdated?.Invoke(this, EventArgs.Empty);
			progressDialog.Show(false);

			var succeededGuids = new List<Guid>();
			var failedGuids = new List<Guid>();
			foreach (var orderReservation in orderReservations)
			{
				try
				{
					progressDialog.AddProgressLine($"Retrieving order {orderReservation.Name}[{orderReservation.ID}]...");
					var order = helpers.OrderManager.GetOrder(orderReservation);
					progressDialog.AddProgressLine($"Order Retrieved");

					progressDialog.AddProgressLine($"Updating UI properties for Order {orderReservation.Name}[{orderReservation.ID}]...");
					order.UpdateUiProperties(helpers);
					progressDialog.AddProgressLine($"Order UI properties updated");

					bool serviceConfigUpdateRequired = false;
					if (updateServices)
					{
						foreach (var service in order.AllServices)
						{
							if (service.RecordingConfiguration.IsConfigured && String.IsNullOrWhiteSpace(service.RecordingConfiguration.RecordingName))
							{
								progressDialog.AddProgressLine($"Updating Recording Configuration for Recording {service.Name}[{service.Id}]...");
								service.RecordingConfiguration.RecordingName = order.Name;
								service.RecordingConfiguration.IsConfigured = true;

								if (service.UpdateRecordingConfigurationProperty(helpers, order))
								{
									progressDialog.AddProgressLine($"Recording Configuration updated");
								}
								else
								{
									progressDialog.AddProgressLine($"Failed to update Recording Configuration");
								}

								serviceConfigUpdateRequired = true;
							}

							progressDialog.AddProgressLine($"Updating UI properties for {service.Name}[{service.Id}]...");
							if (service.UpdateUiProperties(helpers, order))
							{
								progressDialog.AddProgressLine($"UI properties updated");
							}
							else
							{
								progressDialog.AddProgressLine($"Failed to update UI properties");
							}
						}
					}

					if (serviceConfigUpdateRequired)
					{
						progressDialog.AddProgressLine($"Updating Service Configuration for Order {orderReservation.Name}[{orderReservation.ID}]...");
						if (order.UpdateServiceConfigurationProperty(helpers))
						{
							progressDialog.AddProgressLine($"Order Service Configuration updated");
						}
						else
						{
							progressDialog.AddProgressLine($"Failed to update Order Service Configuration");
						}
					}

					succeededGuids.Add(order.Id);
				}
				catch (Exception e)
				{
					progressDialog.AddProgressLine($"Something went wrong with updating order {orderReservation.ID} {e}");
					failedGuids.Add(orderReservation.ID);
				}
			}

			ShowRequestResult($"Updated UI Properties {DateTime.Now.ToShortTimeString()}", $"Succeeded Guids:\n{string.Join("\n", succeededGuids)}\n\nFailed Guids:\n{string.Join("\n", failedGuids)}");

			progressDialog.Finish();
			ShowProgressDialog?.Invoke(this, progressDialog);
		}

		private void ShowRequestResult(string header, params string[] results)
		{
			responseSections.Add(new RequestResultSection(header, results));

			GenerateUi();
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(updateUiPropertiesLabel, ++row, 0, 1, 5);

			AddSection(getOrderReservationsSection, ++row, 0);
			row += getOrderReservationsSection.RowCount;

			AddWidget(updateRecordingServicesCheckBox, ++row, 0, 1, 5);

			AddWidget(updateUiPropertiesButton, ++row, 0, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 1);

			row++;
			foreach (var responseSection in responseSections)
			{
				responseSection.Collapse();
				AddSection(responseSection, row, 0);
				row += responseSection.RowCount;
			}
		}
	}
}
