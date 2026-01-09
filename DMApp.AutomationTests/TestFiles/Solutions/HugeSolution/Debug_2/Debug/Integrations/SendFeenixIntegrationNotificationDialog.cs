namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations
{
	using System;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Feenix;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.YLE.Integrations.Feenix;

	public class SendFeenixIntegrationNotificationDialog : DebugDialog
	{
		private readonly Label feenixLabel = new Label("Feenix") { Style = TextStyle.Heading };

		private readonly Label feenixIdLabel = new Label("ID");
		private readonly YleTextBox feenixIdTextBox = new YleTextBox { ValidationText = "Invalid Guid", ValidationPredicate = text => Guid.TryParse(text, out var result) };
		private readonly Button randomIdButton = new Button("Enter Random Guid") { Width = 150 };

		private readonly Label finnishTitleLabel = new Label("Finnish Title");
		private readonly YleTextBox finnishTitleTextBox = new YleTextBox();

		private readonly Label swedishTitleLabel = new Label("Swedish Title");
		private readonly YleTextBox swedishTitleTextBox = new YleTextBox();

		private readonly Label samiTitleLabel = new Label("Sami Title");
		private readonly YleTextBox samiTitleTextBox = new YleTextBox();

		private readonly Label startLabel = new Label("Start");
		private readonly DateTimePicker startDateTimePicker = new DateTimePicker(DateTime.Now.AddHours(1));

		private readonly Label endLabel = new Label("End");
		private readonly DateTimePicker endDateTimePicker = new DateTimePicker(DateTime.Now.AddHours(2));

		private readonly Label sourceResourceLabel = new Label("Source Resource Name");
		private readonly TextBox sourceResourceTextBox = new TextBox();

		private readonly Label destinationResourceLabel = new Label("Destination Resource Name");
		private readonly TextBox destinationResourceTextBox = new TextBox();

		private readonly Label ceitonProjectIdLabel = new Label("Ceiton Project ID");
		private readonly TextBox ceitonProjectIdTextBox = new TextBox();

		private readonly Label ceitonProductIdLabel = new Label("Ceiton Product ID");
		private readonly TextBox ceitonProductIdTextBox = new TextBox();

		private readonly Button sendNotificationButton = new Button("Send Notification") { Width = 200 };

		private readonly Button sendStopNotificationButton = new Button("Send Stop Notification") { Width = 200 };

		private readonly Button deleteLiveStreamOrderButton = new Button("Delete Live Stream Order") { Width = 200 };

		public SendFeenixIntegrationNotificationDialog(Utilities.Helpers helpers) : base(helpers)
		{
			Title = "Send Feenix Integration Notification";

			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			randomIdButton.Pressed += (s, e) => feenixIdTextBox.Text = Guid.NewGuid().ToString();
			sendNotificationButton.Pressed += SendNotificationButton_Pressed;
			sendStopNotificationButton.Pressed += SendStopNotificationButton_Pressed;
			deleteLiveStreamOrderButton.Pressed += DeleteLiveStreamOrderButton_Pressed;
		}

		private void SendNotificationButton_Pressed(object sender, EventArgs e)
		{
			if (!feenixIdTextBox.IsValid) return;

			var notification = new FeenixNotification
			{
				Id = Guid.Parse(feenixIdTextBox.Text),
				FinnishTitle = finnishTitleTextBox.Text,
				SwedishTitle = swedishTitleTextBox.Text,
				SamiTitle = samiTitleTextBox.Text,
				Start = startDateTimePicker.DateTime,
				End = endDateTimePicker.DateTime,
				SourceResourceName = sourceResourceTextBox.Text,
				DestinationResourceName = destinationResourceTextBox.Text,
				CeitonProjectId = ceitonProjectIdTextBox.Text,
				CeitonProductId = ceitonProductIdTextBox.Text,
			};

			string notficationString = notification.ToString();
			DataMinerInterface.IDmsElement.SetParameter(helpers, helpers.OrderManagerElement.FeenixElement, FeenixProtocol.LastReceivedNotificationParameterId, notficationString);

			ShowRequestResult($"Sent Notification {notification.Id}", JsonConvert.SerializeObject(notification, Formatting.Indented), notficationString);
		}

		private void SendStopNotificationButton_Pressed(object sender, EventArgs e)
		{
			if (!feenixIdTextBox.IsValid) return;

			var notification = new FeenixStopNotification(feenixIdTextBox.Text);

			string notficationString = notification.ToString();
			DataMinerInterface.IDmsElement.SetParameter(helpers, helpers.OrderManagerElement.FeenixElement, FeenixProtocol.LastReceivedNotificationParameterId, notficationString);

			ShowRequestResult($"Sent Notification {notification.Id}", JsonConvert.SerializeObject(notification, Formatting.Indented), notficationString);
		}

		private void DeleteLiveStreamOrderButton_Pressed(object sender, EventArgs e)
		{
			if (!feenixIdTextBox.IsValid) return;

			OrderManagerElement orderManagerElement = new OrderManagerElement(helpers);
			if (orderManagerElement.FeenixElement == null || orderManagerElement.FeenixElement.State != ElementState.Active)
			{
				ShowRequestResult($"Unable to remove Live Stream Order {feenixIdTextBox.Text}", "Feenix element was not linked with order manager or not active");
				return;
			}

			// Remove entry from Feenix element
			var liveStreamOrdersTable = orderManagerElement.FeenixElement.GetTable(FeenixElement.LiveStreamOrdersTableParameterId);
			if (liveStreamOrdersTable.RowExists(feenixIdTextBox.Text))
			{
				liveStreamOrdersTable.DeleteRow(feenixIdTextBox.Text);
			}

			// Trigger integration update
			orderManagerElement.Element.SetParameter(150, feenixIdTextBox.Text);

			ShowRequestResult($"Removed Live Stream Order {feenixIdTextBox.Text}", "Removed");
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 5);

			AddWidget(feenixLabel, ++row, 0, 1, 5);

			AddWidget(feenixIdLabel, ++row, 0);
			AddWidget(feenixIdTextBox, row, 1);
			AddWidget(randomIdButton, row, 2);

			AddWidget(finnishTitleLabel, ++row, 0);
			AddWidget(finnishTitleTextBox, row, 1);

			AddWidget(swedishTitleLabel, ++row, 0);
			AddWidget(swedishTitleTextBox, row, 1);

			AddWidget(samiTitleLabel, ++row, 0);
			AddWidget(samiTitleTextBox, row, 1);

			AddWidget(startLabel, ++row, 0);
			AddWidget(startDateTimePicker, row, 1);

			AddWidget(endLabel, ++row, 0);
			AddWidget(endDateTimePicker, row, 1);

			AddWidget(sourceResourceLabel, ++row, 0);
			AddWidget(sourceResourceTextBox, row, 1);

			AddWidget(destinationResourceLabel, ++row, 0);
			AddWidget(destinationResourceTextBox, row, 1);

			AddWidget(ceitonProjectIdLabel, ++row, 0);
			AddWidget(ceitonProjectIdTextBox, row, 1);

			AddWidget(ceitonProductIdLabel, ++row, 0);
			AddWidget(ceitonProductIdTextBox, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);

			AddWidget(sendNotificationButton, ++row, 0, 1, 5);

			AddWidget(sendStopNotificationButton, ++row, 0, 1, 5);

			AddWidget(deleteLiveStreamOrderButton, ++row, 0, 1, 5);

			AddResponseSections(row + 1);
		}
	}
}
