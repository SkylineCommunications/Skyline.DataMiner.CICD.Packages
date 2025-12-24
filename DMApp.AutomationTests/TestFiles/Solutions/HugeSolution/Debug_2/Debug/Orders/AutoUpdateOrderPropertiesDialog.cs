namespace Debug_2.Debug.Orders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Debug_2.Debug.Reservations;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class AutoUpdateOrderPropertiesDialog : DebugDialog
	{
		private readonly Label headerLabel = new Label("Automatically Update Order Properties") { Style = TextStyle.Title };

		private readonly List<Skyline.DataMiner.Library.Solutions.SRM.Model.Properties.Property> orderBookingManagerProperties;

		private readonly GetReservationsSection getReservationsSection;

		private readonly Label propertiesToUpdateLabel = new Label("Properties to Update");
		private readonly CheckBoxList propertiesToUpdateCheckBoxList;

		private readonly Button updatePropertiesButton = new Button("Update selected properties for selected orders") { Style = ButtonStyle.CallToAction };

		public AutoUpdateOrderPropertiesDialog(Helpers helpers) : base(helpers)
		{
			Title = "Update Order Properties";

			getReservationsSection = new GetReservationsSection(helpers);

			getReservationsSection.AddDefaultPropertyFilter("Type", "Video"); // to ensure we are retrieving order reservations
			getReservationsSection.RegenerateUiRequired += (o, e) => RegenerateUi();

			var bookingManagerElement = helpers.Engine.FindElement("Order Booking Manager") ?? throw new NotFoundException("Could not find Order Booking Manager element");
			var bookingManager = new BookingManager((Engine)helpers.Engine, bookingManagerElement) { CustomProperties = true };

			orderBookingManagerProperties = bookingManager.Properties.ToList();

			propertiesToUpdateCheckBoxList = new CheckBoxList(orderBookingManagerProperties.Select(p => p.Name).ToList());

			updatePropertiesButton.Pressed += UpdatePropertiesButton_Pressed;

			GenerateUi();
		}

		private void RegenerateUi()
		{
			getReservationsSection.RegenerateUi();
			GenerateUi();
		}

		private void UpdatePropertiesButton_Pressed(object sender, EventArgs e)
		{
			using (UiDisabler.StartNew(this))
			{
				var enabledProperties = orderBookingManagerProperties.Where(p => propertiesToUpdateCheckBoxList.Checked.Contains(p.Name)).ToList();

				var orders = getReservationsSection.SelectedReservations.Select(r => helpers.OrderManager.GetOrder(r)).ToList();

				foreach (var order in orders)
				{
					var propertiesToUpdate = order.GetPropertiesFromBookingManager(helpers, enabledProperties).ToDictionary(p => p.Name, p => (object)p.Value);

					order.TryUpdateCustomProperties(helpers, propertiesToUpdate);
				}

				ShowRequestResult("Updated order properties", $"Updated properties\n{string.Join(", ", enabledProperties.Select(p => p.Name))}\nfor orders\n{string.Join("\n", orders.Select(o => o.Name))}");
				GenerateUi();
			}		
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(headerLabel, ++row, 0, 1, 3);

			AddSection(getReservationsSection, ++row, 0);
			row += getReservationsSection.RowCount;

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(propertiesToUpdateLabel, ++row, 0, verticalAlignment: VerticalAlignment.Top);
			AddWidget(propertiesToUpdateCheckBoxList, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(updatePropertiesButton, ++row, 0, 1, 3);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddResponseSections(row);
		}
	}
}
